using FlipLeaf;
using FlipLeaf.Readers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PathfinderFr
{
    public class Program
    {
        public static void Main(string[] args) => FlipLeafProgram.Main<Startup>(args);
    }

    public class Startup : FlipLeafStartup
    {
        public Startup(IConfiguration config)
            : base(config)
        {
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            services.AddCustomIdentity<Website.PathfinderFrWebsiteIdentity>();

            services.AddSingletonAllInterfaces<Markup.WikiMarkup>();
            services.AddSingleton<IContentReader, Readers.WikiContentReader>();

            services.AddSingleton(Config.GetSection("PathfinderFr").Get<PathfinderFrSettings>());
        }
    }
}
