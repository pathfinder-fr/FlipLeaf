using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FlipLeaf
{
    public interface IRenderingMiddleware
    {
        void Render(RenderContext context, Action<RenderContext> next);
    }

    public class RenderContext
    {
        public IDictionary<string, object> Values { get; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public Stream Input { get; set; }

        public Stream Output { get; set; }
    }

    public class AppEngine
    {
        private IRenderingMiddleware[] _middlewares;
        private RenderContext _context;
        private int index;

        public AppEngine(IEnumerable<IRenderingMiddleware> middlewares)
        {
            _middlewares = middlewares.ToArray();
        }

        public void Execute(Stream input, Stream output)
        {
            _context = new RenderContext { Input = input, Output = output };
            ExecuteNext(this._context);

            var first = this._middlewares[index];

            first.Render(this._context, this.ExecuteNext);
        }

        private void ExecuteNext(RenderContext context)
        {
            if (this.index >= this._middlewares.Length)
            {
                // terminé
                return;
            }

            // swap input/output
            var previousInput = this._context.Input;
            this._context.Input = this._context.Output;

            var next = this._middlewares[this.index];
            this.index++;
            next.Render(this._context, this.ExecuteNext);


        }
    }
}
