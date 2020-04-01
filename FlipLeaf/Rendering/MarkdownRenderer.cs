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
        private readonly MarkdownPipelineBuilder _pipelineBuilder = new MarkdownPipelineBuilder();

        private MarkdownPipeline _pipeline;

        public MarkdownRenderer(FlipLeafSettings settings)
        {
            _pipelineBuilder.Extensions.AddIfNotAlready(new WikiLinkExtension() { Extension = ".md" });
            _pipelineBuilder.Extensions.AddIfNotAlready(new CustomLinkInlineRendererExtension(settings.BaseUrl));
        }

        public string Render(string markdown)
        {
            if (_pipeline == null)
            {
                _pipeline = _pipelineBuilder.UseAdvancedExtensions().Build();
            }

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
