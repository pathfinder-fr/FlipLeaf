using FlipLeaf.Storage;
using FlipLeaf.Website;
using Fluid;
using Fluid.Filters;
using Fluid.Values;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace FlipLeaf.Markup
{
    public interface ILiquidMarkup
    {
        ValueTask<string> RenderAsync(string content, HeaderFieldDictionary headers, IWebsite website, out TemplateContext templateContext);

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

                    var parser = new FluidParser();
                    parser.RegisterEmptyTag("body", async (writer, encoder, context) =>
                    {
                        if (context.AmbientValues.TryGetValue("body", out var body))
                        {
                            await writer.WriteAsync((string)body).ConfigureAwait(false);
                        }
                        else
                        {
                            throw new ParseException("Could not render body, Layouts can't be evaluated directly.");
                        }

                        return Fluid.Ast.Completion.Normal;
                    });

                    IFluidTemplate template;
                    try
                    {
                        template = parser.Parse(content);
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
            var parser = new FluidParser();
            var pageTemplate = parser.Parse(content);

            // prepare context
            templateContext = CreateTemplateContext();
            templateContext.SetValue(KnownVariables.Page, headers);
            templateContext.SetValue(KnownVariables.Site, website);

            // render content
            return pageTemplate.RenderAsync(templateContext);
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

            layoutContext.AmbientValues.Add("body", source);

            // render layout
            source = await layout.RenderAsync(layoutContext).ConfigureAwait(false);

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
            var options = new TemplateOptions();
            options.MemberAccessStrategy.Register(typeof(Website.IWebsite));
            options.MemberAccessStrategy.Register(typeof(Website.Website));
            options.Filters.AddFilter("relative_url", RelativeUrlFilterAsync);
            options.FileProvider = _fileProvider;

            return new TemplateContext(options);
        }

        private ValueTask<FluidValue> RelativeUrlFilterAsync(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            return string.IsNullOrEmpty(_baseUrl)
                ? ValueTask.FromResult(input)
                : StringFilters.Prepend(input, new FilterArguments(new StringValue(_baseUrl)), context);
        }

        public class LiquidFile(IStorageItem file)
        {
            public virtual string Name => File.RelativePath;

            protected IStorageItem File { get; } = file;

            public override int GetHashCode() => File.GetHashCode();

            public override bool Equals(object? obj) => obj switch
            {
                LiquidFile item => File.Equals(item.File),
                _ => base.Equals(obj)
            };
        }

        public class LiquidInclude(IStorageItem file, byte[] content) : LiquidFile(file)
        {
            public new IStorageItem File => base.File;

            public byte[] Content { get; } = content;
        }

        public class LiquidLayout(IStorageItem file, HeaderFieldDictionary yamlHeader, IFluidTemplate template) : LiquidFile(file)
        {

            public override string Name { get; } = Path.GetFileNameWithoutExtension(file.Name);

            public HeaderFieldDictionary YamlHeader { get; } = yamlHeader;

            public ValueTask<string> RenderAsync(TemplateContext context) => template.RenderAsync(context);

            public override int GetHashCode() => Name.GetHashCode();

            public override string ToString() => Name;
        }

        private class FlipLeafFileProvider(IDictionary<string, LiquidInclude> includes) : IFileProvider
        {
            public IDirectoryContents GetDirectoryContents(string subpath) => NotFoundDirectoryContents.Singleton;

            public IFileInfo GetFileInfo(string subpath) => includes.TryGetValue(subpath, out var include) ? new IncludeFileInfo(include) : new NotFoundFileInfo(subpath);

            public IChangeToken Watch(string filter) => NullChangeToken.Singleton;

            private class IncludeFileInfo(LiquidInclude include) : IFileInfo
            {
                private readonly byte[] _content = include.Content;

                public bool Exists => true;

                public long Length => 0;

                public string PhysicalPath { get; } = include.File.FullPath;

                public string Name { get; } = include.File.Name;

                public DateTimeOffset LastModified => DateTime.MinValue;

                public bool IsDirectory => false;

                public Stream CreateReadStream() => new MemoryStream(_content);
            }
        }
    }
}
