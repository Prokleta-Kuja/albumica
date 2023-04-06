
using System.Text.Json;
using System.Text.Json.Serialization;
using albumica.Entities;
using albumica.Extensions;
using albumica.Services;
using Hangfire;
using Hangfire.Common;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using poshtar.Extensions;
using Serilog;
using Serilog.Events;

namespace albumica;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(C.IsDebug ? LogEventLevel.Debug : LogEventLevel.Information)
                .MinimumLevel.Override(nameof(Microsoft), LogEventLevel.Warning)
                .MinimumLevel.Override(nameof(Hangfire), LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog();
            builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.All);
            builder.Services.AddDataProtection().PersistKeysToDbContext<AppDbContext>();
            switch (C.DbContextType)
            {
                case DbContextType.PostgreSQL: builder.Services.AddDbContext<AppDbContext, PostgresDbContext>(); break;
                case DbContextType.SQLite: builder.Services.AddDbContext<AppDbContext, SqliteDbContext>(); break;
            }

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.UseOneOfForPolymorphism();
                options.UseAllOfForInheritance();

                options.DescribeAllParametersInCamelCase();
                options.SchemaFilter<OpenApiEnumSchemaFilter>();
                options.SupportNonNullableReferenceTypes();
                options.UseAllOfToExtendReferenceSchemas();
            });

            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(opt =>
                {
                    opt.Cookie.Name = ".albumica.auth";
                    opt.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
                    opt.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
                });

            builder.Services.AddControllers(opt =>
                {
                    opt.Filters.Add<ExceptionFilter>();
                })
                .ConfigureApiBehaviorOptions(opt =>
                {
                    opt.InvalidModelStateResponseFactory = BadRequestFactory.Handle;
                })
                .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull;
                    o.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                    o.JsonSerializerOptions.WriteIndented = true;
                });
            // In production, the React files will be served from this directory
            builder.Services.AddSpaStaticFiles(c => { c.RootPath = "client-app"; });

            // Add Hangfire services.
            builder.Services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSQLiteStorage(C.Paths.Hangfire, new SQLiteStorageOptions
                {
                    QueuePollInterval = TimeSpan.FromSeconds(15),
                    InvisibilityTimeout = TimeSpan.FromMinutes(30),
                    DistributedLockLifetime = TimeSpan.FromSeconds(30),
                    JobExpirationCheckInterval = TimeSpan.FromHours(1),
                    CountersAggregateInterval = TimeSpan.FromMinutes(5),
                }));

            // Remove Hangfire culture filter
            var captureFilter = GlobalJobFilters.Filters.OfType<JobFilter>().Where(c => c.Instance is CaptureCultureAttribute).FirstOrDefault();
            if (captureFilter != null)
                GlobalJobFilters.Filters.Remove(captureFilter.Instance);

            // Add the processing server as IHostedService
            builder.Services.AddHangfireServer(o =>
            {
                o.ServerName = nameof(albumica);
                o.WorkerCount = Math.Max(2, Environment.ProcessorCount / 2);
            });

            builder.Services.AddHttpClient<HibpService>();
            builder.Services.AddSingleton<IPasswordHasher, PasswordHashingService>();

            var app = builder.Build();
            await Initialize(app.Services);

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
                app.UseForwardedHeaders();

            app.UseSpaStaticFiles();
            if (C.IsDebug) // Reverse proxy will handle the redirection
                app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseJobDashboard();
            app.ReregisterRecurringJobs();

            app.MapControllers().RequireAuthorization();
            app.AddTusEndpoint();

            app.MapWhen(x => !x.Request.Path.Value!.StartsWith("/api/"), builder =>
            {
                builder.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "../client-app";
                    if (app.Environment.IsDevelopment())
                        spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
                });
            });

            Log.Information("App started");
            app.Run();

            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
    static async Task Initialize(IServiceProvider provider)
    {
        Directory.CreateDirectory(C.Paths.ConfigData);
        Directory.CreateDirectory(C.Paths.TempData);
        Directory.CreateDirectory(C.Paths.QueueData);

        using var scope = provider.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (db.Database.GetMigrations().Any())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();

        // Demo data
        if (C.IsDebug && !db.Users.Any())
        {
            var pwd = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            await db.InitializeDefaults(pwd);
        }
    }
}
