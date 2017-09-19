using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FlipLeaf
{
    interface IRenderingMiddleware
    {
        void Render(RenderContext context, IRenderingMiddleware next);
    }

    class RenderContext
    {
        public TextReader Input { get; set; }

        public TextWriter Output { get; set; }
    }
}
