using System.Threading.Tasks;
using FlipLeaf.Storage;

namespace FlipLeaf.Files
{
    public class MarkdownFile : IFileFormat
    {
        private readonly Rendering.IYamlParser _yaml;
        private readonly Rendering.ILiquidRenderer _liquid;
        private readonly Rendering.IMarkdownRenderer _markdown;
        private readonly IFileSystem _fileSystem;

        public MarkdownFile(
            Rendering.IYamlParser yaml,
            Rendering.ILiquidRenderer liquid,
            Rendering.IMarkdownRenderer markdown,
            IFileSystem fileSystem)
        {
            _yaml = yaml;
            _liquid = liquid;
            _markdown = markdown;
            _fileSystem = fileSystem;
        }

        public string Extension => ".md";

        public bool RawAllowed => true;

        public async Task<ParsedFile> RenderAsync(IStorageItem file)
        {
            // 1) read all content
            var content = _fileSystem.ReadAllText(file);

            // 2) parse yaml header
            var yamlHeader = _yaml.ParseHeader(content, out content);

            // 3) parse liquid
            content = await _liquid.RenderAsync(content, yamlHeader, out var context).ConfigureAwait(false);

            // 4) parse markdown (if required...)
            content = _markdown.Render(content);

            // 5) apply liquid layout
            // this call can be recusrive if there are multiple layouts
            content = await _liquid.ApplyLayoutAsync(content, context).ConfigureAwait(false);

            return new ParsedFile(content, yamlHeader, "text/html");
        }
    }
}
