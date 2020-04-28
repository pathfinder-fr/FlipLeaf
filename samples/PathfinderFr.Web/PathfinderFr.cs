using FlipLeaf;
using FlipLeaf.Readers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PathfinderFr
{
    public class PathfinderFr
    {
        public static void Main(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) => config.AddEnvironmentVariables())
            .ConfigureWebHostDefaults(builder => builder.UseWebRoot(@".static").UseStartup<Startup>())
            .Build()
            .Run()
            ;
    }

    public class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IConfiguration config) => _config = config;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddHttpContextAccessor();
            services.AddFlipLeaf(_config, useDefaultWebsiteIdentity: true);

            services.AddSingletonAllInterfaces<Markup.WikiMarkup>();
            services.AddSingleton<IContentReader, Readers.WikiContentReader>();

            services.AddSingleton(_config.GetSection("PathfinderFr").Get<PathfinderFrSettings>());
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
            app.UseEndpoints(endpoints => endpoints.MapRazorPages());
        }
    }
}
