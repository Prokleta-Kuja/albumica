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
            var videos = new DirectoryInfo(C.Settings.VideosRootPath); videos.Create();
            var cache = new DirectoryInfo(C.Settings.CacheRootPath); cache.Create();
            //await Test2();
            // await Test();

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
            if (Debugger.IsAttached && !db.Images.Any())
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

            var info = SixLabors.ImageSharp.Image.Identify(inputFile.FullName);
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
            using var img = await SixLabors.ImageSharp.Image.LoadAsync(inputFile.FullName);
            var opt = new ResizeOptions
            {
                Size = new Size(500, 500),
                Mode = ResizeMode.Crop
            };
            img.Mutate(x =>
            {
                x.Resize(opt);
            });

            // The library automatically picks an encoder based on the file extension then
            // encodes and write the data to disk.
            // You can optionally set the encoder to choose.
            img.Save("bar.jpg");
        }
        static async Task Test2()
        {
            await Task.CompletedTask;
            var img = SixLabors.ImageSharp.Image.Load("vt4.jpg");

            // AutoOrient to avoid boundig box brain fuck
            img.Mutate(x => x.AutoOrient());
            Console.WriteLine($"x: {img.Width} y: {img.Height}");

            // After AutoOrient exif is no longer needed
            img.Metadata.ExifProfile = null;

            // Resize for Azure long edge not greater then 1920
            var azure = new ResizeOptions
            {
                Size = new Size(1920, 1920),
                Mode = ResizeMode.Max,
            };
            var azureImg = img.Clone(x => x.Resize(azure));
            Console.WriteLine($"x: {img.Width} y: {img.Height}");
            Console.WriteLine($"x: {azureImg.Width} y: {azureImg.Height}");

            //img.Save("for_azure.jpg");


            // Resize for viewport
            var viewport = new ResizeOptions
            {
                Size = new Size(1920, 1080),
                Mode = ResizeMode.Max,
            };
            var viewportImg = img.Clone(x => x.Resize(viewport));
            Console.WriteLine($"x: {img.Width} y: {img.Height}");
            Console.WriteLine($"x: {viewportImg.Width} y: {viewportImg.Height}");

            //img.Save("for_viewport.jpg");

            // Calculate bounding box
            /*
function findScreenCoords(mouseEvent)
{
  if (mouseEvent)
  {
    console.log("x",mouseEvent.pageX,"y",mouseEvent.pageY);
  }
}
document.getElementsByTagName("img")[0].onmousemove = findScreenCoords;

Image size
x 3880 y 5184 Original
x 1437 y 1920 AzureOptimized

Ratio
3880/1437=2,7001

Point of interest
x 920 y 792
x 340 y 294

Get resized POI from original
920/2,7001 = 340,7281
792/2,7001 = 293,3225

Get original POI from resized
340*2,7001 = 919,x
293*2,7001 = 791,x
            */
        }
    }
}
