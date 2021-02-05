using System;
using System.Collections.Generic;
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
        ValueTask<string> RenderAsync(string content, HeaderFieldDictionary headers, IWebsite website, out TemplateContext templateContext);

        LayoutTemplate ParseLayout(string layout);

        ValueTask<string> ApplyLayoutAsync(string content, TemplateContext contentContext, IWebsite website);
    }

    public class LiquidMarkup : ILiquidMarkup, IWebsiteComponent
    {
        private readonly Dictionary<string, LiquidLayout> _layouts = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, LiquidInclude> _includes = new(StringComparer.OrdinalIgnoreCase);
        private readonly FlipLeafFileProvider _fileProvider;
        private readonly string _baseUrl;
        private readonly IYamlMarkup _yaml;

        public LiquidMarkup(FlipLeafSettings settings, IYamlMarkup yaml)
        {
            _baseUrl = settings.BaseUrl;
            _fileProvider = new FlipLeafFileProvider(_includes);
            _yaml = yaml;
        }

        public void OnLoad(IFileSystem fileSystem, IWebsite website)
        {
            IStorageItem? dirItem;

            // populate Layouts
            _layouts.Clear();
            dirItem = fileSystem.GetItem(KnownFolders.Layouts);
            if (dirItem != null && fileSystem.DirectoryExists(dirItem))
            {
                foreach (var file in fileSystem.GetFiles(dirItem, pattern: "*.html"))
                {
                    var content = fileSystem.ReadAllText(file);

                    HeaderFieldDictionary? yamlHeader;
                    try
                    {
                        yamlHeader = _yaml.ParseHeader(content, out content);
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException($"Layout {file} is invalid: YAML errors", nameof(file), ex);
                    }

                    LayoutTemplate template;
                    try
                    {
                        template = this.ParseLayout(content);
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException($"Layout {file} in invalid: Liquid errors", nameof(file), ex);
                    }

                    var layout = new LiquidLayout(file, yamlHeader, template);
                    _layouts.Add(layout.Name, layout);
                }
            }

            // populate includes
            _includes.Clear();
            dirItem = fileSystem.GetItem(KnownFolders.Includes);
            if (dirItem != null && fileSystem.DirectoryExists(dirItem))
            {
                foreach (var file in fileSystem.GetFiles(dirItem, pattern: "*.liquid"))
                {
                    byte[] content;

                    using (var openRead = fileSystem.OpenRead(file))
                    using (var ms = new MemoryStream())
                    {
                        openRead.CopyTo(ms);
                        content = ms.ToArray();
                    }

                    var include = new LiquidInclude(file, content);
                    _includes.Add(file.Name, include);
                }
            }
        }

        public ValueTask<string> RenderAsync(string content, HeaderFieldDictionary headers, IWebsite website, out TemplateContext templateContext)
        {
            // parse content as template
            var pageTemplate = PageTemplate.Parse(content);

            // prepare context
            templateContext = CreateTemplateContext();
            templateContext.SetValue(KnownVariables.Page, headers);
            templateContext.SetValue(KnownVariables.Site, website);

            // render content
            return pageTemplate.RenderAsync(templateContext);
        }

        public LayoutTemplate ParseLayout(string content)
        {
            return LayoutTemplate.Parse(content);
        }

        public async ValueTask<string> ApplyLayoutAsync(string source, TemplateContext sourceContext, IWebsite website)
        {
            var pageItem = sourceContext.GetValue(KnownVariables.Page);
            var layout = await pageItem.GetValueAsync(KnownVariables.Layout, sourceContext).ConfigureAwait(false);
            var layoutName = layout.ToStringValue();
            if (string.IsNullOrEmpty(layoutName))
            {
                return source; // no layout field, ends here
            }

            return await ApplyLayoutAsync(source, sourceContext, website, layoutName, 0).ConfigureAwait(false);
        }

        private async ValueTask<string> ApplyLayoutAsync(string source, TemplateContext sourceContext, IWebsite website, string layoutName, int level)
        {
            if (level >= 5)
            {
                // no more than x levels of nesting
                throw new NotSupportedException($"Recursive layouts are limited to 5 levels of recursion");
            }


            // load layout
            if (!_layouts.TryGetValue(layoutName, out var layout))
            {
                return source;
            }

            // create new TemplateContext for the layout
            var layoutContext = CreateTemplateContext();
            layoutContext.SetValue(KnownVariables.Page, sourceContext.GetValue(KnownVariables.Page));
            layoutContext.SetValue(KnownVariables.Layout, layout.YamlHeader);
            layoutContext.SetValue(KnownVariables.Site, website);

            layoutContext.AmbientValues.Add(LayoutTemplate.BodyAmbientValueKey, source);

            // render layout
            source = await layout.Template.RenderAsync(layoutContext).ConfigureAwait(false);

            if (!layout.YamlHeader.TryGetValue(KnownVariables.Layout, out var outerLayoutObject) || !(outerLayoutObject is string outerLayoutFile))
            {
                // no recusrive layout, we stop here
                return source;
            }

            // recursive layout...
            return await ApplyLayoutAsync(source, sourceContext, website, outerLayoutFile, level + 1).ConfigureAwait(false);
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
            templateContext.MemberAccessStrategy.Register(typeof(Website.Website));
            templateContext.Filters.AddFilter("relative_url", RelativeUrlFilter);
            templateContext.FileProvider = _fileProvider;

            return templateContext;
        }

        private FluidValue RelativeUrlFilter(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var baseUrl = _baseUrl;
            if (string.IsNullOrEmpty(baseUrl))
            {
                return input;
            }

            return StringFilters.Prepend(input, new FilterArguments(new StringValue(baseUrl)), context);
        }
    }
}
