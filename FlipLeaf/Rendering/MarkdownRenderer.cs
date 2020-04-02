using System.IO;
using FlipLeaf.Rendering.Markdown;
using Markdig;
using Markdig.Renderers;

namespace FlipLeaf.Rendering
{
    public interface IMarkdownRenderer
    {
        string Render(string markdown);
    }

    public class MarkdownRenderer : IMarkdownRenderer
    {
        private readonly MarkdownPipeline _pipeline;

        public MarkdownRenderer(FlipLeafSettings settings)
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
