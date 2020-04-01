using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FlipLeaf
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host
                .CreateDefaultBuilder(args)                
                // configuration
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddEnvironmentVariables();
                })
                // defaults & startup
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseWebRoot(@".static"); // hardcoded :(
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
