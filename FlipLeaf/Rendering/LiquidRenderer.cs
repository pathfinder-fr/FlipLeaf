using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FlipLeaf.Rendering.Liquid;
using FlipLeaf.Storage;
using Fluid;
using Fluid.Filters;
using Fluid.Values;

namespace FlipLeaf.Rendering
{
    public interface ILiquidRenderer
    {
        ValueTask<string> RenderAsync(string content, IDictionary<string, object> pageContext, out TemplateContext templateContext);

        ValueTask<string> ApplyLayoutAsync(string source, TemplateContext sourceContext);
    }

    public class LiquidRenderer : ILiquidRenderer
    {
        private readonly ConcurrentDictionary<string, LayoutCache?> _layoutCache = new ConcurrentDictionary<string, LayoutCache?>();
        private readonly FlipLeafSettings _settings;
        private readonly IYamlParser _yaml;
        private readonly FlipLeafFileProvider _fileProvider;
        private readonly IFileSystem _fileSystem;

        public LiquidRenderer(FlipLeafSettings settings, IYamlParser yaml, IFileSystem fileSystem)
        {
            _settings = settings;
            _yaml = yaml;
            _fileSystem = fileSystem;
            _fileProvider = new FlipLeafFileProvider(settings);
        }

        public ValueTask<string> RenderAsync(string content, IDictionary<string, object> yamlHeader, out TemplateContext templateContext)
        {
            // parse content as template
            var template = PageTemplate.Parse(content);

            // prepare context
            templateContext = CreateTemplateContext();
            templateContext.SetValue("page", yamlHeader);

            // render content
            return template.RenderAsync(templateContext);
        }

        public async ValueTask<string> ApplyLayoutAsync(string source, TemplateContext sourceContext)
        {
            var pageItem = sourceContext.GetValue("page");
            var layout = await pageItem.GetValueAsync("layout", sourceContext).ConfigureAwait(false);
            var layoutFile = layout.ToStringValue();
            if (string.IsNullOrEmpty(layoutFile))
            {
                return source; // no layout field, ends here
            }

            return await ApplyLayoutAsync(source, sourceContext, layoutFile, 0).ConfigureAwait(false);
        }

        private async ValueTask<string> ApplyLayoutAsync(string source, TemplateContext sourceContext, string layoutFile, int level)
        {
            if (level >= 5)
            {
                // no more than 10 levels of nesting
                throw new NotSupportedException($"Recursive layouts are limited to 5 levels of recursion");
            }

            if (string.IsNullOrEmpty(Path.GetExtension(layoutFile)))
            {
                layoutFile += ".html";
            }

            // load layout
            var layoutCache = LoadLayout(layoutFile);
            if (layoutCache == null)
            {
                return source;
            }

            // create new TemplateContext for the layout
            var layoutContext = CreateTemplateContext();
            layoutContext.SetValue("page", sourceContext.GetValue("page"));
            layoutContext.SetValue("layout", layoutCache.YamlHeader);
            layoutContext.AmbientValues.Add(LayoutTemplate.BodyAmbientValueKey, source);

            // render layout
            source = await layoutCache.ViewTemplate.RenderAsync(layoutContext).ConfigureAwait(false);

            if (!layoutCache.YamlHeader.TryGetValue("layout", out var outerLayoutObject) || !(outerLayoutObject is string outerLayoutFile))
            {
                // no recusrive layout, we stop here
                return source;
            }

            // recursive layout...
            return await ApplyLayoutAsync(source, sourceContext, outerLayoutFile, level + 1).ConfigureAwait(false);
        }

        private LayoutCache? LoadLayout(string fileName)
        {
            return CreateLayout(fileName);
            //return _layoutCache.GetOrAdd(fileName, CreateLayout);
        }

        private LayoutCache? CreateLayout(string fileName)
        {
            var layoutPath = Path.Combine("_layouts", fileName);
            var layoutItem = _fileSystem.GetItem(layoutPath);
            if (layoutItem == null || !_fileSystem.FileExists(layoutItem))
            {
                return null;
            }

            var layoutText = _fileSystem.ReadAllText(layoutItem);

            IDictionary<string, object>? yamlHeader;
            try
            {
                yamlHeader = _yaml.ParseHeader(layoutText, out layoutText);
            }
            catch(Exception ex)
            {
                throw new ArgumentOutOfRangeException($"Layout {fileName} is invalid", ex);
            }

            var layoutTemplate = LayoutTemplate.Parse(layoutText);

            return new LayoutCache(layoutTemplate, yamlHeader);
        }

        private TemplateContext CreateTemplateContext()
        {
            var templateContext = new TemplateContext
            {
                MemberAccessStrategy = new MemberAccessStrategy
                {
                    IgnoreCasing = true,
                    MemberNameStrategy = MemberNameStrategies.Default
                }
            };

            templateContext.Filters.AddFilter("relative_url", RelativeUrlFilter);
            templateContext.FileProvider = _fileProvider;

            return templateContext;
        }

        private FluidValue RelativeUrlFilter(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var baseUrl = _settings.BaseUrl;
            if (string.IsNullOrEmpty(baseUrl))
            {
                return input;
            }

            return StringFilters.Prepend(input, new FilterArguments(new StringValue(baseUrl)), context);
        }

        private class LayoutCache
        {
            public LayoutCache(LayoutTemplate viewTemplate, IDictionary<string, object> yamlHeader)
            {
                ViewTemplate = viewTemplate;
                YamlHeader = yamlHeader;
            }

            public IDictionary<string, object> YamlHeader { get; }

            public LayoutTemplate ViewTemplate { get; }
        }
    }
}
