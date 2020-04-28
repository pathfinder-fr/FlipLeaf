using PathfinderFr.Docs;

namespace PathfinderFr.Markup.WikiFormatter
{
    internal interface IWikiPagesProvider
    {
        string GetPageTitle(WikiPage info, bool v);

        WikiPage FindPage(string destination);

        WikiSnippet FindSnippet(string snippetName);
    }

    internal class WikiPagesProvider : IWikiPagesProvider
    {
        private readonly IWiki _wiki;

        public WikiPagesProvider(IWiki wiki)
        {
            _wiki = wiki;
        }

        public string GetPageTitle(WikiPage info, bool v) => _wiki.GetPage((WikiName)info.Name.FullName)?.Title;

        public WikiPage FindPage(string destination) => _wiki.GetPage((WikiName)destination);

        public WikiSnippet FindSnippet(string snippetName) => _wiki.GetSnippet(snippetName);
    }

}
