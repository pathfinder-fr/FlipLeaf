using Fluid;

namespace FlipLeaf.Services.FluidLiquid
{
    public class ViewTemplate : BaseFluidTemplate<ViewTemplate>
    {
        static ViewTemplate()
        {
            Factory.RegisterTag<RenderBodyTag>("renderbody");
        }
    }
}
