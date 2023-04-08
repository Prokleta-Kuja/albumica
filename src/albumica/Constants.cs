using System.Globalization;
using System.Security.Authentication;

namespace albumica;

public static class C
{
    public static readonly bool IsDebug;
    public const string ADMIN_ROLE = "admin";
    public static readonly string PostgresConnectionString;
    public static readonly DbContextType DbContextType;
    public static readonly TimeZoneInfo TZ;
    public static readonly CultureInfo Locale;
    static C()
    {
        IsDebug = Environment.GetEnvironmentVariable("DEBUG") == "1";
        PostgresConnectionString = Environment.GetEnvironmentVariable("POSTGRES") ?? string.Empty;
        DbContextType = !string.IsNullOrWhiteSpace(PostgresConnectionString) ? DbContextType.PostgreSQL : DbContextType.SQLite;

        try
        {
            TZ = TimeZoneInfo.FindSystemTimeZoneById(Environment.GetEnvironmentVariable("TZ") ?? "Europe/Zagreb");
        }
        catch (Exception)
        {
            TZ = TimeZoneInfo.Local;
        }

        try
        {
            Locale = CultureInfo.GetCultureInfo(Environment.GetEnvironmentVariable("LOCALE") ?? "en-UK");
        }
        catch (Exception)
        {
            Locale = CultureInfo.InvariantCulture;
        }

    }
    public static class Paths
    {
        static string Root => IsDebug ? Path.Join(Environment.CurrentDirectory, "/data") : "/data";
        public const string MediaRequest = "/media";
        public static string MediaRequestFor(string file) => $"{MediaRequest}/{file}";
        public static readonly string ConfigData = $"{Root}/config";
        public static string ConfigDataFor(string file) => Path.Combine(ConfigData, file);
        public static readonly string MediaData = $"{Root}/media";
        public static string MediaDataFor(string file) => Path.Combine(MediaData, file);
        public static readonly string QueueData = $"{Root}/queue";
        public static string QueueDataFor(string file) => Path.Combine(QueueData, file);
        public static readonly string Sqlite = ConfigDataFor("app.db");
        public static readonly string Hangfire = ConfigDataFor("queue.db");
        public static readonly string AppDbConnectionString = $"Data Source={Sqlite}";
        public static readonly string HangfireConnectionString = $"Data Source={Hangfire}";
    }
}

public enum DbContextType
{
    SQLite,
    PostgreSQL,
}