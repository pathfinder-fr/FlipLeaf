using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FlipLeaf.Rendering.Liquid;
using Fluid;
using Fluid.Filters;
using Fluid.Values;

namespace FlipLeaf.Rendering
{
    public interface ILiquidRenderer
    {
        ValueTask<string> RenderAsync(string content, IDictionary<string, object> pageContext, out TemplateContext templateContext);

        ValueTask<string> ApplyLayoutAsync(string content, TemplateContext context);
    }

    public class LiquidRenderer : ILiquidRenderer
    {
        private readonly ConcurrentDictionary<string, Task<ViewTemplate>> _layoutCache = new ConcurrentDictionary<string, Task<ViewTemplate>>();
        private readonly FlipLeafSettings _settings;
        private readonly FlipLeafFileProvider _fileProvider;

        public LiquidRenderer(FlipLeafSettings settings)
        {
            _settings = settings;
            _fileProvider = new FlipLeafFileProvider(settings);
        }

        public ValueTask<string> RenderAsync(string content, IDictionary<string, object> pageContext, out TemplateContext templateContext)
        {
            // parse content as template
            if (!ViewTemplate.TryParse(content, out var template))
            {
                throw new ParseException();
            }

            // prepare context
            templateContext = new TemplateContext
            {
                MemberAccessStrategy = new MemberAccessStrategy
                {
                    IgnoreCasing = true,
                    MemberNameStrategy = MemberNameStrategies.Default
                }
            };

            templateContext.Filters.AddAsyncFilter("relative_url", FlipLeafFilters.RelativeUrl);
            templateContext.FileProvider = _fileProvider;
            //context.MemberAccessStrategy.Register<WebSiteConfiguration>();
            //context.MemberAccessStrategy.Register<WebSite>();
            templateContext.SetValue("page", pageContext);
            //context.SetValue("site", _ctx.Runtime);

            // render content
            return template.RenderAsync(templateContext);
        }

        public async ValueTask<string> ApplyLayoutAsync(string source, TemplateContext context)
        {
            var page = context.GetValue("page");

            var layout = await page.GetValueAsync("layout", context).ConfigureAwait(false);

            var layoutFile = layout.ToStringValue();
            if (string.IsNullOrEmpty(layoutFile))
            {
                return source;
            }

            if (string.IsNullOrEmpty(Path.GetExtension(layoutFile)))
            {
                layoutFile += ".html";
            }

            var layoutTemplate = await LoadLayout(layoutFile).ConfigureAwait(false);

            context.AmbientValues.Add(RenderBodyTag.BodyAmbientValueKey, source);

            try
            {
                source = await layoutTemplate.RenderAsync(context).ConfigureAwait(false);
            }
            finally
            {
                context.AmbientValues.Remove(RenderBodyTag.BodyAmbientValueKey);
            }

            return source;
        }

        private Task<ViewTemplate> LoadLayout(string fileName) => _layoutCache.GetOrAdd(fileName, CreateLayout);

        private async Task<ViewTemplate> CreateLayout(string fileName)
        {
            string layoutText;
            using (var reader = new StreamReader(Path.Combine(_settings.SourcePath, ".layouts", fileName)))
            {
                layoutText = await reader.ReadToEndAsync().ConfigureAwait(false);
            }

            if (!ViewTemplate.TryParse(layoutText, out var layoutTemplate))
            {
                throw new ParseException();
            }

            return layoutTemplate;
        }
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

    
}
