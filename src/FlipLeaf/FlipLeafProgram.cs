using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FlipLeaf
{
    /// <summary>
    /// Contains default program entry point for a FlipLeaf app.
    /// </summary>
    public static class FlipLeafProgram
    {
        /// <summary>
        /// Default program entry point for a standard FlipLeaf app.
        /// </summary>
        public static void Main(string[] args) => Main<FlipLeafStartup>(args);

        /// <summary>
        /// Default program entry point for a standard FlipLeaf app, with a custom <typeparamref name="TStartup"/> startup class.
        /// </summary>
        public static void Main<TStartup>(string[] args)
            where TStartup : class
        {
            var startConsole = false;
            if (args != null && args.Length != 0 && args[0] == "console")
            {
                startConsole = true;
            }

            if (!startConsole)
            {
                Host
                    .CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((hostingContext, config) => config.AddEnvironmentVariables())
                    .ConfigureWebHostDefaults(builder => builder.UseWebRoot(@".static").UseStartup<TStartup>())
                    .Build()
                    .Run();

                return;
            }

            // TODO : start console version
            System.Console.WriteLine("TODO: console");
            Host
                    .CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((hostingContext, config) => config.AddEnvironmentVariables())
                    .ConfigureServices((ctx, services) =>
                    {
                        var startup = new FlipLeafStartup(ctx.Configuration);
                        startup.ConfigureServices(services);
                        services.AddSingleton<IHostedService, ConsoleHostingService>();
                    })
                    .RunConsoleAsync(o => { o.SuppressStatusMessages = false; });
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
