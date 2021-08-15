using System.Diagnostics;
using albumica.Configuration;
using albumica.Data;
using albumica.FaceRecognition;
using albumica.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace albumica
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AiOptions>(Configuration.GetSection(AiOptions.Section));
            services.AddTransient<IFaceRecogniton, NoFaceRecognition>();
            services.AddHttpClient();

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddDbContext<AppDbContext>(builder =>
             {
                 builder.UseSqlite(C.Settings.AppDbConnectionString);
                 if (Debugger.IsAttached)
                 {
                     builder.EnableSensitiveDataLogging();
                     builder.LogTo(message => Debug.WriteLine(message), new[] { RelationalEventId.CommandExecuted });
                 }
             });

            services.AddDataProtection().PersistKeysToDbContext<AppDbContext>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseImageResizer(C.Routes.Resizer, C.Settings.ImagesRootPath, C.Settings.CacheRootPath);
            app.UseImageResizer(C.Routes.ResizerImport, C.Settings.ImportRootPath, C.Settings.CacheImportPath);

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
