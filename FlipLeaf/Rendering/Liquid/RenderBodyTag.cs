using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;

namespace FlipLeaf.Rendering.Liquid
{
    public class RenderBodyTag : SimpleTag
    {
        internal const string Tag = "renderbody";
        internal const string BodyAmbientValueKey = "body";

        public override async ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (context.AmbientValues.TryGetValue(BodyAmbientValueKey, out var body))
            {
                await writer.WriteAsync((string)body).ConfigureAwait(false);
            }
            else
            {
                throw new ParseException("Could not render body, Layouts can't be evaluated directly.");
            }

            return Completion.Normal;
        }
    }
}
