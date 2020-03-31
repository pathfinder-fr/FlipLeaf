using FlipLeaf.Services;
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
            bool useDefaultWebsite = false)
        {

            var settings = configuration.GetSection(section).Get<FlipLeafSettings>() ?? new FlipLeafSettings();
            services.AddSingleton(settings);

            services.AddSingleton<IGitService, GitService>();
            services.AddSingleton<IMarkdownService, MarkdownService>();
            services.AddSingleton<ILiquidService, LiquidService>();
            services.AddSingleton<IYamlService, YamlService>();
            services.AddSingleton<IFormTemplateService, FormTemplateService>();

            if (useDefaultWebsite)
            {
                services.AddSingleton<IWebsite, DefaultWebsite>();
            }
        }
    }
}
