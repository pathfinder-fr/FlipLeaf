using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FlipLeaf
{
    public class MarkdownRenderer : IRenderingMiddleware
    {
        public void Render(RenderContext context, Action<RenderContext> next)
        {
            using (var reader = new StreamReader(context.Input))
            {
                var htmlContent = Markdig.Markdown.ToHtml(reader.ReadToEnd());

                context.Output = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent));
            }

            next?.Invoke(context);
        }
    }
}
