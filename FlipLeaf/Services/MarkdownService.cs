using FlipLeaf.Services.Markdown;

namespace FlipLeaf.Services
{
    public interface IMarkdownService
    {
        string Parse(string markdown);
    }

    public class MarkdownService : IMarkdownService
    {
        private readonly MarkdigParser _parser;

        public MarkdownService(FlipLeafSettings settings)
        {
            _parser = new MarkdigParser();
            _parser.Use(new WikiLinkExtension() { Extension = ".md" });
            _parser.Use(new CustomLinkInlineRendererExtension(settings.BaseUrl));
        }

        public string Parse(string markdown)
        {
            return _parser.Parse(markdown);
        }
    }
}
