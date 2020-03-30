using System.Threading.Tasks;
using Fluid;
using Fluid.Filters;
using Fluid.Values;

namespace FlipLeaf.Services.FluidLiquid
{
    public static class FlipLeafFilters
    {
        public static async ValueTask<FluidValue> RelativeUrl(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var site = context.GetValue("site");
            var baseUrl = await site.GetValueAsync("baseUrl", context);

            if (baseUrl.IsNil())
            {
                return input;
            }

            return StringFilters.Prepend(input, new FilterArguments(new StringValue(baseUrl.ToStringValue())), context);
        }
    }
}
