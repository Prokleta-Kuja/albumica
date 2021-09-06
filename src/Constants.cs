using System;
using System.IO;

namespace albumica
{
    public static class C
    {
        public static class Env
        {
            public static string Locale => Environment.GetEnvironmentVariable("LOCALE") ?? "hr-HR";
            public static string TimeZone => Environment.GetEnvironmentVariable("TZ") ?? "Europe/Zagreb";
        }
        public static class Routes
        {
            public const string Root = "/";
            public const string Resizer = "/img";
            public const string ResizerImport = "/img-import";
            public static string ResizerImportFor(string relativePath) => $"{ResizerImport}/{relativePath}";
            public const string People = "/people";
            public const string Person = "/people/{Id:int}";
            public static string PersonsFor(int id) => $"/people/{id}";
            public const string Locations = "/locations";
            public const string LocationsCountry = "/locations/country/{Id:int}";
            public static string LocationsCountryFor(int id) => $"/locations/country/{id}";
            public const string LocationsCity = "/locations/city/{Id:int}";
            public static string LocationsCityFor(int id) => $"/locations/city/{id}";
            public const string LocationsSuburb = "/locations/suburb/{Id:int}";
            public static string LocationsSuburbFor(int id) => $"/locations/suburb/{id}";
            public const string Import = "/import";
            public const string Images = "/images";
            public const string Invoices = "/invoices";
            public const string Invoice = "/invoices/{Id:int}";
            public static string InvoiceFor(int id) => $"{Invoices}/{id}";
        }
        public static class Settings
        {
            public static string DataPath(string file) => Path.Combine(Environment.CurrentDirectory, "appdata", file);
            public static readonly string ImportRootPath = DataPath("import");
            public static readonly string ImagesRootPath = DataPath("images");
            public static readonly string VideosRootPath = DataPath("videos");
            public static readonly string CacheRootPath = DataPath("cache");
            public static readonly string CacheImportPath = Path.Combine(CacheRootPath, "import");
            public static string ImagesForPath(DateTime created, string fileName) => DataPath($"images/{created.Year}/{created.Month}/{fileName}");
            public static string VideosForPath(DateTime created, string fileName) => DataPath($"videos/{created.Year}/{created.Month}/{fileName}");
            public static readonly string AppDbConnectionString = $"Data Source={DataPath("app.db")}";
        }
    }
}