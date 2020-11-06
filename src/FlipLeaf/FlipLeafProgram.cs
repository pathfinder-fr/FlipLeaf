using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FlipLeaf
{
    public static class FlipLeafProgram
    {
        public static void Main(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) => config.AddEnvironmentVariables())
            .ConfigureWebHostDefaults(builder => builder.UseWebRoot(@".static").UseStartup<Startup>())
            .Build()
            .Run()
            ;
    }
}
