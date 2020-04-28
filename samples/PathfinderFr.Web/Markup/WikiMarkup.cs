using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FlipLeaf;
using FlipLeaf.Markup;
using FlipLeaf.Storage;
using FlipLeaf.Website;
using PathfinderFr.Docs;
using PathfinderFr.Markup.WikiFormatter;

namespace PathfinderFr.Markup
{
    public interface IWiki
    {
        WikiPage GetPage(WikiName name);

        WikiPage GetPage(IStorageItem file, HeaderFieldDictionary headers);

        WikiSnippet GetSnippet(string name);

        string Render(string content, WikiPage page);
    }

    public class WikiMarkup : IWiki, IWebsiteComponent
    {
        private readonly IFileSystem _fileSystem;
        private readonly IYamlMarkup _yaml;
        private readonly IDictionary<string, WikiSnippet> _snippets = new Dictionary<string, WikiSnippet>(StringComparer.OrdinalIgnoreCase);

        public WikiMarkup(IFileSystem fileSystem, IYamlMarkup yaml)
        {
            _fileSystem = fileSystem;
            _yaml = yaml;
        }

        public void OnLoad(IFileSystem fileSystem, IDocumentStore docs)
        {
            // chargement snippets
            var snippets = fileSystem.GetItem("_snippets");
            foreach (var snippetFile in fileSystem.GetFiles(snippets, pattern: "*.wiki"))
            {
                var snippetContent = fileSystem.ReadAllText(snippetFile);
                _snippets[snippetFile.Name] = new WikiSnippet(snippetContent);
            }
        }

        public WikiPage GetPage(WikiName name)
        {
            var item = _fileSystem.GetItem($"{name.Namespace}/{name.Name}.wiki");
            if (!_fileSystem.FileExists(item))
            {
                return null;
            }

            using (var reader = _fileSystem.OpenTextReader(item))
            {
                if (_yaml.TryParseHeader(reader, out var headers, out _))
                {
                    return GetPage(item, headers);
                }
            }

            return null;
        }

        public WikiPage GetPage(IStorageItem file, HeaderFieldDictionary headers)
        {
            var dateText = (string)headers.GetValueOrDefault("LastModified");

            return new WikiPage(
                file,
                (string)headers["Name"],
                (string)headers["Title"],
                dateText != null ? DateTimeOffset.ParseExact(dateText, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) : DateTimeOffset.Now,
                headers.GetArray<string>("Categories").ToArray()
                );
        }

        public WikiSnippet GetSnippet(string name)
            => _snippets.TryGetValue(name, out var snippet) ? snippet : null;

        public string Render(string content, WikiPage page)
        {
            var formatter = new Formatter(new WikiPagesProvider(this));
            content = formatter.Format(content, false, FormattingContext.PageContent, new PageInfo() { FullName = page.FullName });

            return content;
        }
    }
}
