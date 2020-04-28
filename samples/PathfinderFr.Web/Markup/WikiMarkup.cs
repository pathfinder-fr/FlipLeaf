using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FlipLeaf;
using FlipLeaf.Markup;
using FlipLeaf.Storage;
using FlipLeaf.Website;
using Microsoft.Extensions.Logging;
using PathfinderFr.Docs;
using PathfinderFr.Markup.WikiFormatter;

namespace PathfinderFr.Markup
{
    public interface IWiki
    {
        WikiPage GetPage(WikiName name);

        IEnumerable<WikiName> GetAllCategories();

        IEnumerable<WikiName> GetCategoryPages(WikiName category);

        WikiPage GetPage(IStorageItem file, HeaderFieldDictionary headers);

        WikiSnippet GetSnippet(string name);

        string Render(string content, WikiPage page, out WikiName redirect);
    }

    public class WikiMarkup : IWiki, IWebsiteComponent
    {
        private readonly ILogger<WikiMarkup> _log;
        private readonly IFileSystem _fileSystem;
        private readonly IYamlMarkup _yaml;
        private readonly IDictionary<string, WikiSnippet> _snippets = new Dictionary<string, WikiSnippet>(StringComparer.OrdinalIgnoreCase);
        private readonly IDictionary<IStorageItem, WikiPage> _pages = new Dictionary<IStorageItem, WikiPage>();
        private readonly IDictionary<WikiName, List<WikiName>> _categories = new Dictionary<WikiName, List<WikiName>>();

        public WikiMarkup(IFileSystem fileSystem, IYamlMarkup yaml, ILogger<WikiMarkup> log)
        {
            _log = log;
            _fileSystem = fileSystem;
            _yaml = yaml;
        }

        public void OnLoad(IFileSystem fileSystem, IDocumentStore docs)
        {
            // chargement snippets
            var snippets = fileSystem.GetItem("_snippets");
            foreach (var snippetFile in fileSystem.GetFiles(snippets, pattern: "*.txt"))
            {
                var snippetContent = fileSystem.ReadAllText(snippetFile);
                _snippets[System.IO.Path.GetFileNameWithoutExtension(snippetFile.Name)] = new WikiSnippet(snippetContent);
            }

            // chargement infos pages & catégories
            var root = fileSystem.GetItem(null);
            FillPages(fileSystem, root, null);
            foreach (var dir in fileSystem.GetSubDirectories(root))
            {
                FillPages(fileSystem, dir, dir.Name);
            }
        }

        public void FillPages(IFileSystem fileSystem, IStorageItem directory, string @namespace)
        {
            foreach (var file in fileSystem.GetFiles(directory, pattern: "*.txt"))
            {
                var page = GetPage(file);
                if (page != null)
                {
                    _pages[file] = page;
                    if (page.Categories != null && page.Categories.Length > 0)
                    {
                        foreach (var categoryName in page.Categories)
                        {
                            var category = new WikiName(page.Name.Namespace, categoryName);
                            if (!_categories.TryGetValue(category, out var pages))
                            {
                                _categories[category] = pages = new List<WikiName>();
                            }

                            pages.Add(page.Name);
                        }
                    }
                }
            }
        }

        public WikiPage GetPage(WikiName name)
        {
            if (name.Namespace.StartsWith("c:"))
            {
                // page de catégories, non géré pour l'instant
                return null;
            }

            var file = _fileSystem.GetItem($"{name.Namespace}/{name.Name}.txt");
            if (!_fileSystem.FileExists(file))
            {
                return null;
            }

            return GetPage(file);
        }

        private WikiPage GetPage(IStorageItem file)
        {
            if (_pages.TryGetValue(file, out var page))
            {
                return page;
            }

            using (var reader = _fileSystem.OpenTextReader(file))
            {
                try
                {
                    if (_yaml.TryParseHeader(reader, out var headers, out _))
                    {
                        return GetPage(file, headers);
                    }
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Impossible d'analyser la page {0}, page ignorée", file);
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

        public string Render(string content, WikiPage page, out WikiName redirect)
        {
            var pageInfo = new PageInfo { FullName = page.Name.FullName };

            var formatter = new Formatter(new WikiPagesProvider(this));

            content = formatter.Format(content, false, FormattingContext.PageContent, pageInfo, out _, out redirect);

            if (redirect != null)
            {
                return null;
            }

            content = formatter.FormatPhase3(content, FormattingContext.PageContent, pageInfo);

            return content;
        }

        public IEnumerable<WikiName> GetCategoryPages(WikiName category)
        {
            if (!_categories.TryGetValue(category, out var pages))
            {
                return Enumerable.Empty<WikiName>();
            }

            return pages;
        }

        public IEnumerable<WikiName> GetAllCategories() => _categories.Keys;
    }
}
