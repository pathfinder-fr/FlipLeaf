using FlipLeaf.Storage;
using FlipLeaf.Website;

namespace FlipLeaf.Readers
{
    public class HtmlContentReader : IContentReader
    {
        private readonly Markup.IYamlMarkup _yaml;
        private readonly Markup.ILiquidMarkup _liquid;
        private readonly IFileSystem _fileSystem;
        private readonly IWebsite _website;

        public HtmlContentReader(
            Markup.IYamlMarkup yaml,
            Markup.ILiquidMarkup liquid,
            IFileSystem fileSystem,
            IWebsite website)
        {
            _yaml = yaml;
            _liquid = liquid;
            _fileSystem = fileSystem;
            _website = website;
        }

        public bool AcceptRequest(IStorageItem requestFile, out IStorageItem diskFile)
        {
            diskFile = requestFile;
            if (requestFile.IsHtml())
            {
                return _fileSystem.FileExists(requestFile);
            }

            return false;
        }

        public bool AcceptFileAsRequest(IStorageItem diskfile, out IStorageItem requestFile)
        {
            requestFile = diskfile;
            return diskfile.IsHtml();
        }

        public bool RawAllowed => false;

        public Task<HeaderFieldDictionary?> ReadHeaderAsync(IStorageItem file)
        {
            using var reader = _fileSystem.OpenTextReader(file);

            if (!_yaml.TryParseHeader(reader, out var header, out _))
            {
                return Task.FromResult<HeaderFieldDictionary?>(null);
            }

            return Task.FromResult(header);
        }

        public async Task<IReadResult> ReadAsync(IStorageItem file)
        {
            // 1) read all content
            var content = _fileSystem.ReadAllText(file);

            // 2) parse yaml header
            var yamlHeader = _yaml.ParseHeader(content, out content);

            // 3) parse liquid
            content = await _liquid.RenderAsync(content, yamlHeader, _website, out var context).ConfigureAwait(false);

            // 5) apply liquid layout
            // this call can be recusrive if there are multiple layouts
            content = await _liquid.ApplyLayoutAsync(content, context, _website).ConfigureAwait(false);

            return new ContentReadResult(content, yamlHeader, "text/html");
        }
    }
}
