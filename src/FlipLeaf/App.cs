using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FlipLeaf
{
    /// <summary>
    /// Contains default program entry point for a FlipLeaf app.
    /// </summary>
    public static class App
    {
        /// <summary>
        /// Default program entry point for a standard FlipLeaf app.
        /// </summary>
        public static Task Run(string[] args, Action<IServiceCollection, IConfiguration> configureServices = null)
        {
            var startConsole = false;
            if (args != null && args.Length != 0 && args[0] == "console")
            {
                startConsole = true;
            }

            if (!startConsole)
            {
                var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args, WebRootPath = ".static" });

                builder.Services.AddRazorPages();
                builder.Services.AddHttpContextAccessor();
                builder.Services.AddFlipLeaf(builder.Configuration);
                builder.Services.Configure<Microsoft.AspNetCore.Routing.RouteOptions>(o => o.LowercaseUrls = true);

                configureServices?.Invoke(builder.Services, builder.Configuration);

                var app = builder.Build();

#if DEBUG
                app.UseDeveloperExceptionPage();
#endif

                app.UseStaticFiles();
                app.UseAuthorization();
                app.MapRazorPages();
                app.UseFlipLeaf(app.Environment);

                return app.RunAsync();
            }
            else
            {
                // TODO : start console version
                Console.WriteLine("TODO: console");

                var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { Args = args, ContentRootPath = ".static" });
                
// TODO Migrer

                return Host
                    .CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((hostingContext, config) => config.AddEnvironmentVariables())
                    .ConfigureServices((ctx, services) =>
                    {
                        services.AddRazorPages();
                        services.AddHttpContextAccessor();
                        services.AddFlipLeaf(ctx.Configuration);
                        services.Configure<Microsoft.AspNetCore.Routing.RouteOptions>(o => o.LowercaseUrls = true);
                        configureServices?.Invoke(services, ctx.Configuration);
                        services.AddSingleton<IHostedService, ConsoleHostingService>();
                    })
                    .RunConsoleAsync(o => { o.SuppressStatusMessages = false; });
            }
        }

        class ConsoleHostingService : IHostedService
        {
            //private readonly IRazorViewEngine _viewEngine;
            //private readonly ITempDataProvider _tempDataProvider;
            //private readonly IServiceProvider _serviceProvider;

            public ConsoleHostingService(
                //IRazorViewEngine viewEngine
                ITempDataProvider tempDataProvider,
                IServiceProvider serviceProvider
                )
            {
                //_viewEngine = viewEngine;
                //_tempDataProvider = tempDataProvider;
                //_serviceProvider = serviceProvider;
            }

            //public ConsoleHostingService()
            //{
            //}

            public Task StartAsync(CancellationToken cancellationToken)
            {
                System.Console.WriteLine("Start");
                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
