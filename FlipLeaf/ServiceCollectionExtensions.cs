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

            // storage
            services.AddSingleton<Storage.IFileSystem, Storage.FileSystem>();
            services.AddSingleton<Storage.IGitRepository, Storage.GitRepository>();

            // markup
            services.AddSingleton<Markup.IMarkdownMarkup, Markup.MarkdownMarkup>();
            services.AddSingleton<Markup.ILiquidMarkup, Markup.LiquidMarkup>();
            services.AddSingleton<Markup.IYamlMarkup, Markup.YamlMarkup>();

            // templating
            services.AddSingleton<Templating.IFormTemplateParser, Templating.FormTemplateParser>();

            // readers
            services.AddSingleton<Readers.IContentReader, Readers.HtmlContentReader>();
            services.AddSingleton<Readers.IContentReader, Readers.MarkdownContentReader>();
            services.AddSingleton<Readers.IContentReader, Readers.JsonContentReader>();

            services.AddSingleton<Readers.IDataReader, Readers.JsonDataReader>();

            services.AddSingleton<Website.IWebsite, Website.DefaultWebsite>();

            if (useDefaultWebsiteIdentity)
            {
                services.AddSingleton<Website.IWebsiteIdentity, Website.DefaultWebsiteIdentity>();
            }
        }
    }
}
