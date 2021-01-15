using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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
        }
    }
}
