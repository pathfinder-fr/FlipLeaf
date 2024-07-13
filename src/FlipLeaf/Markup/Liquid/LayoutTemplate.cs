using System.Text.Encodings.Web;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;

namespace FlipLeaf.Markup.Liquid
{
    public class LayoutTemplate : BaseFluidTemplate<LayoutTemplate>
    {
        internal const string BodyTag = "body";
        internal const string BodyAmbientValueKey = "body";

        static LayoutTemplate()
        {
            Factory.RegisterTag<RenderBodyTag>(BodyTag);
        }

        private class RenderBodyTag : SimpleTag
        {
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
}

