using FlipLeaf;

await App.Run(args, (s, c) =>
{
    s.AddCustomIdentity<PathfinderFr.Website.PathfinderFrWebsiteIdentity>();
    s.AddSingletonAllInterfaces<PathfinderFr.Markup.WikiMarkup>();
    s.AddSingleton<FlipLeaf.Readers.IContentReader, PathfinderFr.Readers.WikiContentReader>();
    s.Configure<PathfinderFr.PathfinderFrSettings>(c.GetSection("PathfinderFr"));
});

namespace PathfinderFr
{
    public sealed class PathfinderFrSettings
    {
        public string AuthCookieName { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public string DecryptionKey { get; set; } = string.Empty;
        public string ValidationKey { get; set; } = string.Empty;
    }
}