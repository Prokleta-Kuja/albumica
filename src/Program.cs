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
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace albumica
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var import = new DirectoryInfo(C.Settings.ImportRootPath); import.Create();
            var images = new DirectoryInfo(C.Settings.ImagesRootPath); images.Create();
            var cache = new DirectoryInfo(C.Settings.CacheRootPath); cache.Create();
            //await Test();

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
            var import = new DirectoryInfo(C.Settings.ImportRootPath);
            var fi = import.GetFiles().First();
            var img = await Image.LoadAsync(fi.FullName);

            var exif = img.Metadata.ExifProfile.Values.ToDictionary(m => m.Tag);
            if (exif.ContainsKey(ExifTag.GPSLatitudeRef))
            {
                var latRef = exif[ExifTag.GPSLatitudeRef].GetValue();
                var lat = exif[ExifTag.GPSLatitude].GetValue() as Rational[];
                var lonRef = exif[ExifTag.GPSLongitudeRef].GetValue();
                var lon = exif[ExifTag.GPSLongitude].GetValue() as Rational[];

                var longitude = ConvertDegreeAngleToDouble(lon!, lonRef!.ToString());
                var latitude = ConvertDegreeAngleToDouble(lat!, latRef!.ToString());
            }
        }

        private static double ConvertDegreeAngleToDouble(Rational[] coordinates, string? exifGpsLatitudeRef)
        {
            return ConvertDegreeAngleToDouble(coordinates[0].ToDouble(), coordinates[1].ToDouble(), coordinates[2].ToDouble(), exifGpsLatitudeRef);
        }
        private static double ConvertDegreeAngleToDouble(double degrees, double minutes, double seconds, string? exifGpsLatitudeRef)
        {
            var result = ConvertDegreeAngleToDouble(degrees, minutes, seconds);
            if (exifGpsLatitudeRef == "S")
                result = -1 * result;

            return result;
        }
        private static double ConvertDegreeAngleToDouble(double degrees, double minutes, double seconds)
        {
            return degrees + (minutes / 60) + (seconds / 3600);
        }
    }
}
