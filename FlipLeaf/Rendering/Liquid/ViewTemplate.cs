using Fluid;

namespace FlipLeaf.Rendering.Liquid
{
    public class ViewTemplate : BaseFluidTemplate<ViewTemplate>
    {
        static ViewTemplate()
        {
            Factory.RegisterTag<RenderBodyTag>(RenderBodyTag.Tag);
        }
    }
}
