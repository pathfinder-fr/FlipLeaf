using System.Threading.Tasks;
using FlipLeaf.Storage;

namespace FlipLeaf.Files
{
    public class HtmlFile : IFileFormat
    {
        private readonly Rendering.IYamlParser _yaml;
        private readonly Rendering.ILiquidRenderer _liquid;
        private readonly IFileSystem _fileSystem;

        public HtmlFile(
            Rendering.IYamlParser yaml,
            Rendering.ILiquidRenderer liquid,
            IFileSystem fileSystem)
        {
            _yaml = yaml;
            _liquid = liquid;
            _fileSystem = fileSystem;
        }

        public string Extension => ".html";

        public bool RawAllowed => false;

        public async Task<ParsedFile> RenderAsync(IStorageItem file)
        {
            // 1) read all content
            var content = _fileSystem.ReadAllText(file);

            // 2) parse yaml header
            var yamlHeader = _yaml.ParseHeader(content, out content);

            // 3) parse liquid
            content = await _liquid.RenderAsync(content, yamlHeader, out var context).ConfigureAwait(false);

            // 5) apply liquid layout
            // this call can be recusrive if there are multiple layouts
            content = await _liquid.ApplyLayoutAsync(content, context).ConfigureAwait(false);

            return new ParsedFile(content, yamlHeader, "text/html");
        }
    }
}
