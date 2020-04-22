using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlipLeaf
{
    public static class ServiceCollectionExtensions
    {
        public static void AddFlipLeaf(
            this IServiceCollection services,
            IConfiguration configuration,
            string section = "FlipLeaf",
            bool useDefaultWebsiteIdentity = false)
        {

            var settings = configuration.GetSection(section).Get<FlipLeafSettings>() ?? new FlipLeafSettings();
            services.AddSingleton(settings);

            services.AddSingleton<Storage.IFileSystem, Storage.FileSystem>();
            services.AddSingleton<Storage.IGitRepository, Storage.GitRepository>();

            services.AddSingleton<Rendering.IMarkdownRenderer, Rendering.MarkdownRenderer>();
            services.AddSingleton<Rendering.ILiquidRenderer, Rendering.LiquidRenderer>();
            services.AddSingleton<Rendering.IYamlParser, Rendering.YamlParser>();
            services.AddSingleton<Rendering.IFormTemplateParser, Rendering.FormTemplateParser>();

            services.AddSingleton<Website.IWebsite, Website.DefaultWebsite>();

            if (useDefaultWebsiteIdentity)
            {
                services.AddSingleton<Website.IWebsiteIdentity, Website.DefaultWebsiteIdentity>();
            }
        }
    }
}
