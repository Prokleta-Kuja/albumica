using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using albumica.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;

namespace albumica
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var import = new DirectoryInfo(C.Settings.ImportRootPath); import.Create();
            var images = new DirectoryInfo(C.Settings.ImagesRootPath); images.Create();
            var cache = new DirectoryInfo(C.Settings.CacheRootPath); cache.Create();
            await Test();

            await InitializeDb(args);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        static async Task InitializeDb(string[] args)
        {
            var dbFile = new FileInfo(C.Settings.DataPath("app.db"));
            dbFile.Directory?.Create();

            var opt = new DbContextOptionsBuilder<AppDbContext>();
            opt.UseSqlite(C.Settings.AppDbConnectionString);

            var db = new AppDbContext(opt.Options);
            if (db.Database.GetMigrations().Any())
                await db.Database.MigrateAsync();
            else
                await db.Database.EnsureCreatedAsync();

            // Seed
            if (Debugger.IsAttached && !db.Tags.Any())
            {
                await db.ProvisionDemoAsync();
            }
        }
        static async Task Test()
        {
            // TODO: Call as startup
            Configuration.Default.MemoryAllocator = ArrayPoolMemoryAllocator.CreateWithModeratePooling();
            // TODO: call daily at night
            Configuration.Default.MemoryAllocator.ReleaseRetainedResources();

            var import = new DirectoryInfo(C.Settings.ImportRootPath);
            var inputFile = import.GetFiles().First();

            var info = Image.Identify(inputFile.FullName);
            if (info == null)
            {
                // TODO: this is video most likely
                return;
            }

            // Extract GPS coordinates
            var exif = info.Metadata.ExifProfile.Values.ToDictionary(m => m.Tag);
            var coordinateKeys = new ExifTag[] { ExifTag.GPSLatitudeRef, ExifTag.GPSLatitude, ExifTag.GPSLongitude };
            var altitudeKeys = new ExifTag[] { ExifTag.GPSAltitude, ExifTag.GPSAltitudeRef };

            if (coordinateKeys.All(k => exif.ContainsKey(k)))
            {
                var lat = exif[ExifTag.GPSLatitude].GetValue() as Rational[];
                var lon = exif[ExifTag.GPSLongitude].GetValue() as Rational[];
                var latRef = exif[ExifTag.GPSLatitudeRef].GetValue();

                if (lat != null && lon != null && latRef != null)
                {
                    var longitude = lon[0].ToDouble() + (lon[1].ToDouble() / 60) + (lon[2].ToDouble() / 3600);
                    var latitude = lat[0].ToDouble() + (lat[1].ToDouble() / 60) + (lat[2].ToDouble() / 3600);
                    if (latRef.ToString()!.Equals("S", StringComparison.InvariantCultureIgnoreCase))
                        latitude *= -1;
                    // TODO: use

                    // Extract altitude
                    if (altitudeKeys.All(k => exif.ContainsKey(k)))
                    {
                        var alt = (Rational)exif[ExifTag.GPSAltitude].GetValue();
                        var altRef = (byte)exif[ExifTag.GPSAltitudeRef].GetValue();

                        var result = alt.ToDouble();
                        if (altRef == 1)
                            result *= -1; // Below see level

                        // TODO: use
                    }
                }
            }

            // Resize and rotate
            using var img = await Image.LoadAsync(inputFile.FullName);
            var opt = new ResizeOptions();
            opt.Size = new Size(500, 500);
            opt.Mode = ResizeMode.Crop;
            img.Mutate(x =>
            {
                x.Resize(opt);
            });

            // The library automatically picks an encoder based on the file extension then
            // encodes and write the data to disk.
            // You can optionally set the encoder to choose.
            img.Save("bar.jpg");
        }
    }
}
