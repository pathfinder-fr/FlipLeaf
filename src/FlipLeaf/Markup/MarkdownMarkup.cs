using System.IO;
using FlipLeaf.Markup.Markdown;
using Markdig;
using Markdig.Renderers;

namespace FlipLeaf.Markup
{
    public interface IMarkdownMarkup
    {
        string Render(string markdown);
    }

    public class MarkdownMarkup : IMarkdownMarkup
    {
        private readonly MarkdownPipeline _pipeline;

        public MarkdownMarkup(FlipLeafSettings settings)
        {
            var builder = new MarkdownPipelineBuilder();
            builder.Extensions.AddIfNotAlready(new WikiLinkExtension() { Extension = ".md" });
            builder.Extensions.AddIfNotAlready(new CustomLinkInlineRendererExtension(settings.BaseUrl));

            _pipeline = builder.UseAdvancedExtensions().Build();
        }

        public string Render(string markdown)
        {
            using (var writer = new StringWriter())
            {
                var renderer = new HtmlRenderer(writer);
                _pipeline.Setup(renderer);

                var doc = Markdig.Markdown.Parse(markdown, _pipeline);

                renderer.Render(doc);

                writer.Flush();

                return writer.ToString();
            }
        }
    }
}
