using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FlipLeaf;
using FlipLeaf.Markup;
using FlipLeaf.Readers;
using FlipLeaf.Storage;
using FlipLeaf.Website;
using PathfinderFr.Markup;

namespace PathfinderFr.Readers
{
    public class WikiContentReader : IContentReader
    {
        private readonly IWiki _wiki;
        private readonly IYamlMarkup _yaml;
        private readonly IFileSystem _fileSystem;
        private readonly IWebsite _website;

        public WikiContentReader(
            IWiki wiki,
            IYamlMarkup yaml,
            IFileSystem fileSystem,
            IWebsite website)
        {
            _wiki = wiki;
            _yaml = yaml;
            _fileSystem = fileSystem;
            _website = website;
        }

        public bool AcceptFileAsRequest(IStorageItem diskfile, out IStorageItem requestFile)
        {
            if (diskfile.Extension == ".txt")
            {
                requestFile = _fileSystem.ReplaceExtension(diskfile, ".html");
                return true;
            }

            requestFile = diskfile;
            return false;
        }

        public bool AcceptRequest(IStorageItem requestFile, out IStorageItem diskFile)
        {
            diskFile = requestFile;
            if (!requestFile.IsHtml())
            {
                return false;
            }

            // replace default index page by MainPage.html
            var diskPath = requestFile.RelativePath;
            if (requestFile.Name == "index.html")
            {
                diskPath = Regex.Replace(diskPath, @"index\.html$", "MainPage.html", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            }

            // replace dotted filename by directory : Namespace.Page.html  => Namespace/Page.txt
            diskPath = Regex.Replace(diskPath, @"\.(?!html$)", "/", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            diskPath = Regex.Replace(diskPath, @"\.html$", ".txt", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            diskFile = _fileSystem.GetItem(diskPath);
            return _fileSystem.FileExists(diskFile);
        }

        public Task<HeaderFieldDictionary> ReadHeaderAsync(IStorageItem file)
        {
            using var reader = _fileSystem.OpenTextReader(file);

            if (!_yaml.TryParseHeader(reader, out var header, out _))
            {
                return Task.FromResult<HeaderFieldDictionary>(null);
            }

            return Task.FromResult(header);
        }

        public Task<IReadResult> ReadAsync(IStorageItem file)
        {
            // 1) read all content
            var content = _fileSystem.ReadAllText(file);

            // 2) parse yaml header
            var yamlHeader = _yaml.ParseHeader(content, out content);

            yamlHeader["title"] = yamlHeader["Title"];

            // 3) extract wiki page infos
            var page = _wiki.GetPage(file, yamlHeader);

            // 3) parse wiki
            content = _wiki.Render(content, page, out var redirect);

            if (redirect != null)
            {
                return Task.FromResult<IReadResult>(new RedirectReadResult($"{redirect.FullName}.html{redirect.Fragment}"));
            }

            return Task.FromResult<IReadResult>(new ContentReadResult(content, yamlHeader, "text/html"));
        }
    }
}
