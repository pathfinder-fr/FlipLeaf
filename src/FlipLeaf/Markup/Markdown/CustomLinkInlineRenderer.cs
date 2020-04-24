using System;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax.Inlines;

namespace FlipLeaf.Markup.Markdown
{
    public class CustomLinkInlineRendererExtension : IMarkdownExtension
    {
        private readonly string _baseUrl;

        public CustomLinkInlineRendererExtension(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        public void Setup(MarkdownPipelineBuilder pipeline) { }

        public void Setup(MarkdownPipeline pipeline, Markdig.Renderers.IMarkdownRenderer renderer)
        {
            renderer.ObjectRenderers.Replace<LinkInlineRenderer>(new CustomLinkInlineRenderer(_baseUrl));
        }
    }

    public class CustomLinkInlineRenderer : LinkInlineRenderer
    {
        private readonly string _baseUrl;
        private readonly bool _hasBaseUrl;

        public CustomLinkInlineRenderer(string baseUrl)
        {
            _hasBaseUrl = !string.IsNullOrEmpty(baseUrl);
            _baseUrl = baseUrl;
        }

        protected override void Write(HtmlRenderer renderer, LinkInline link)
        {
            link.Url = PrependBasePath(link.Url);

            // quick hack to transform links into their html counterpart
            //
            // should be replaced with a more robust solution to handle extension transformation
            // (moreover if pretty urls must be supported)
            link.Url = link.Url.Replace(".md", ".html");

            base.Write(renderer, link);
        }

        internal string PrependBasePath(string url)
        {
            if (!_hasBaseUrl)
            {
                return url;
            }

            if (Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                return url;
            }

            return _baseUrl + url;
        }
    }
}
