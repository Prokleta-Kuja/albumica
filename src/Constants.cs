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
            public static readonly string CacheRootPath = DataPath("cache");
            public static string ImagesForPath(int year, int month) => DataPath($"images/{year}/{month}");
            public static readonly string AppDbConnectionString = $"Data Source={DataPath("app.db")}";
        }
    }
}