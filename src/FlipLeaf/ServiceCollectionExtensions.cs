using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FlipLeaf
{
    public static class ServiceCollectionExtensions
    {
        public static void AddFlipLeaf(
            this IServiceCollection services,
            IConfiguration configuration,
            string section = "FlipLeaf")
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
            services.AddSingleton<Website.IWebsiteComponent, Templating.FormTemplateManager>();

            // content readers
            services.AddContentReader<Readers.HtmlContentReader>();
            services.AddContentReader<Readers.MarkdownContentReader>();
            services.AddContentReader<Readers.JsonContentReader>();

            // data readers
            services.AddDataReader<Readers.JsonDataReader>();
            services.AddDataReader<Readers.JsonLineDataReader>();

            // website
            services.AddSingleton<Website.IDocumentStore, Website.DocumentStore>();
            services.AddSingletonAllInterfaces<Website.Website>();

            // default empty identity
            services.AddSingleton<Website.IWebsiteIdentity, Website.WebsiteIdentity>();
        }

        public static void AddCustomIdentity<T>(this IServiceCollection services)
            where T : class, Website.IWebsiteIdentity
        {
            services.RemoveAll<Website.IWebsiteIdentity>();
            services.AddSingletonAllInterfaces<T>();
        }

        public static void AddContentReader<T>(this IServiceCollection services) where T : class, Readers.IContentReader
            => services.AddSingleton<Readers.IContentReader, T>();

        public static void AddDataReader<T>(this IServiceCollection services) where T : class, Readers.IDataReader
            => services.AddSingleton<Readers.IDataReader, T>();

        public static void AddSingletonAllInterfaces<T>(this IServiceCollection services)
            where T : class
        {
            services.AddSingleton<T>();
            foreach (var i in typeof(T).GetInterfaces())
            {
                services.AddSingleton(i, s => s.GetRequiredService<T>());
            }
        }
    }
}
