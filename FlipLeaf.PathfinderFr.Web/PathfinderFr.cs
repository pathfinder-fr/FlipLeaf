using FlipLeaf;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PathfinderFr
{
    public class PathfinderFr
    {
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) => config.AddEnvironmentVariables())
            .ConfigureWebHostDefaults(wb
                => wb.UseWebRoot(@".static").UseStartup<Startup>()
            );
    }

    public class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IConfiguration config) => _config = config;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddHttpContextAccessor();
            services.AddSingleton(_config.GetSection("PathfinderFr").Get<PathfinderFrSettings>());
            services.AddFlipLeaf(_config, useDefaultWebsite: true);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
#if DEBUG
            app.UseDeveloperExceptionPage();
#else
            app.UseExceptionHandler("/_site/error");
#endif
            app.UseFlipLeaf(env);
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
