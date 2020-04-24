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
            services.AddSingletonAllInterfaces<Markup.LiquidMarkup>();
            services.AddSingleton<Markup.IYamlMarkup, Markup.YamlMarkup>();

            // templating
            services.AddSingleton<Templating.IFormTemplateParser, Templating.FormTemplateParser>();

            // content readers
            services.AddSingleton<Readers.IContentReader, Readers.HtmlContentReader>();
            services.AddSingleton<Readers.IContentReader, Readers.MarkdownContentReader>();
            services.AddSingleton<Readers.IContentReader, Readers.JsonContentReader>();

            // data readers
            services.AddSingleton<Readers.IDataReader, Readers.JsonDataReader>();

            // website
            services.AddSingletonAllInterfaces<Website.Website>();
            if (useDefaultWebsiteIdentity) services.AddSingleton<Website.IWebsiteIdentity, Website.WebsiteIdentity>();
        }

        public static void AddSingletonAllInterfaces<T>(this IServiceCollection services)
            where T : class
        {
            services.AddSingleton<T>();
            foreach (var i in typeof(T).GetInterfaces())
            {
                services.AddSingleton(i, s => s.GetRequiredService<T>());
            }
        }

        public static void AddSingletonAllInterfaces<TPrimaryService, TImplementation>(this IServiceCollection services)
            where TImplementation : class, TPrimaryService
            where TPrimaryService : class
        {
            services.AddSingleton<TPrimaryService, TImplementation>();
            foreach (var i in typeof(TImplementation).GetInterfaces())
            {
                if (i != typeof(TPrimaryService))
                {
                    services.AddSingleton(i, s => s.GetRequiredService<TPrimaryService>());
                }
            }
        }
    }
}
