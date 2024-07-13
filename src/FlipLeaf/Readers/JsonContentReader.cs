using FlipLeaf.Markup;
using FlipLeaf.Storage;
using FlipLeaf.Website;

namespace FlipLeaf.Readers
{
    public class JsonContentReader : IContentReader
    {
        private readonly IYamlMarkup _yaml;
        private readonly ILiquidMarkup _liquid;
        private readonly IWebsite _website;
        private readonly IFileSystem _fileSystem;

        public JsonContentReader(
            IYamlMarkup yaml,
            ILiquidMarkup liquid,
            IWebsite website,
            IFileSystem fileSystem)
        {
            _yaml = yaml;
            _liquid = liquid;
            _website = website;
            _fileSystem = fileSystem;
        }

        public bool AcceptFileAsRequest(IStorageItem diskfile, out IStorageItem requestFile)
        {
            requestFile = diskfile;

            if (diskfile.Name.EndsWith(".liquid.json"))
            {
                var newRequestFile = _fileSystem.GetItem(diskfile.RelativePath.Replace(".liquid.json", ".json"));
                if (newRequestFile != null)
                {
                    requestFile = newRequestFile;
                    return true;
                }
            }

            return false;
        }

        public bool AcceptRequest(IStorageItem requestFile, out IStorageItem diskFile)
        {
            diskFile = requestFile;

            var liquidFile = _fileSystem.ReplaceExtension(requestFile, ".liquid.json");

            if (!_fileSystem.FileExists(liquidFile))
            {
                return false;
            }

            diskFile = liquidFile;
            return true;
        }

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

            return new ContentReadResult(content, yamlHeader, "text/javascript");
        }
    }
}
