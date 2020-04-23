using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using FlipLeaf.Markup.Liquid;
using FlipLeaf.Storage;
using FlipLeaf.Website;
using Fluid;
using Fluid.Filters;
using Fluid.Values;

namespace FlipLeaf.Markup
{
    public interface ILiquidMarkup
    {
        ValueTask<string> RenderAsync(string content, HeaderFieldDictionary pageContext, out TemplateContext templateContext);

        ValueTask<string> ApplyLayoutAsync(string source, TemplateContext sourceContext);
    }

    public class LiquidMarkup : ILiquidMarkup
    {
        private readonly ConcurrentDictionary<string, LayoutCache?> _layoutCache = new ConcurrentDictionary<string, LayoutCache?>();
        private readonly FlipLeafSettings _settings;
        private readonly IYamlMarkup _yaml;
        private readonly FlipLeafFileProvider _fileProvider;
        private readonly IFileSystem _fileSystem;
        private readonly IWebsite _website;

        public LiquidMarkup(FlipLeafSettings settings, IYamlMarkup yaml, IFileSystem fileSystem, IWebsite website)
        {
            _settings = settings;
            _yaml = yaml;
            _fileSystem = fileSystem;
            _website = website;
            _fileProvider = new FlipLeafFileProvider(settings);
        }

        public ValueTask<string> RenderAsync(string content, HeaderFieldDictionary yamlHeader, out TemplateContext pageContext)
        {
            // parse content as template
            var pageTemplate = PageTemplate.Parse(content);

            // prepare context
            pageContext = CreateTemplateContext();
            pageContext.SetValue(KnownVariables.Page, yamlHeader);
            pageContext.SetValue(KnownVariables.Site, _website);

            // render content
            return pageTemplate.RenderAsync(pageContext);
        }

        public async ValueTask<string> ApplyLayoutAsync(string source, TemplateContext sourceContext)
        {
            var pageItem = sourceContext.GetValue(KnownVariables.Page);
            var layout = await pageItem.GetValueAsync(KnownVariables.Layout, sourceContext).ConfigureAwait(false);
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
                // no more than x levels of nesting
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
            layoutContext.SetValue(KnownVariables.Page, sourceContext.GetValue(KnownVariables.Page));
            layoutContext.SetValue(KnownVariables.Layout, layoutCache.YamlHeader);
            layoutContext.SetValue(KnownVariables.Site, _website);

            layoutContext.AmbientValues.Add(LayoutTemplate.BodyAmbientValueKey, source);

            // render layout
            source = await layoutCache.ViewTemplate.RenderAsync(layoutContext).ConfigureAwait(false);

            if (!layoutCache.YamlHeader.TryGetValue(KnownVariables.Layout, out var outerLayoutObject) || !(outerLayoutObject is string outerLayoutFile))
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
            var layoutPath = Path.Combine(KnownFolders.Layouts, fileName);
            var layoutItem = _fileSystem.GetItem(layoutPath);
            if (layoutItem == null || !_fileSystem.FileExists(layoutItem))
            {
                return null;
            }

            var layoutText = _fileSystem.ReadAllText(layoutItem);

            HeaderFieldDictionary? yamlHeader;
            try
            {
                yamlHeader = _yaml.ParseHeader(layoutText, out layoutText);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Layout {fileName} is invalid: YAML errors", nameof(fileName), ex);
            }

            LayoutTemplate? layoutTemplate;
            try
            {
                layoutTemplate = LayoutTemplate.Parse(layoutText);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Layout {fileName} in invalid: Liquid errors", nameof(fileName), ex);
            }

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

            templateContext.MemberAccessStrategy.Register(typeof(Website.IWebsite));
            templateContext.MemberAccessStrategy.Register(typeof(Website.DefaultWebsite));
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
            public LayoutCache(LayoutTemplate viewTemplate, HeaderFieldDictionary yamlHeader)
            {
                ViewTemplate = viewTemplate;
                YamlHeader = yamlHeader;
            }

            public HeaderFieldDictionary YamlHeader { get; }

            public LayoutTemplate ViewTemplate { get; }
        }
    }
}
