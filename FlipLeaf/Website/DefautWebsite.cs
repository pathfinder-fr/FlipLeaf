using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FlipLeaf.Website
{
    public class DefautWebsite : IWebsite, IWebsiteEventHandler
    {
        private readonly ConcurrentDictionary<string, Template> _templates = new ConcurrentDictionary<string, Template>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, Layout> _layouts = new ConcurrentDictionary<string, Layout>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, Page> _pages = new ConcurrentDictionary<string, Page>(StringComparer.OrdinalIgnoreCase);

        public DefautWebsite()
        {
        }

        public IEnumerable<Template> Templates => _templates.Values;

        public IEnumerable<Layout> Layouts => _layouts.Values;

        public IEnumerable<Page> Pages => _pages.Values;

        public PageCollection Collections { get; }

        public void InvalidatePage(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path.IndexOf('\\') != -1)
            {
                throw new ArgumentOutOfRangeException(nameof(path), path, "Path parameter cannot contains a backslash character");
            }

            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments[0][0] == '_')
            {
                // special
                switch (segments[0].ToLowerInvariant())
                {
                    case "_templates":
                    case "_layouts":
                        break;
                }
            }
            else
            {
                // standard page
            }
        }

        public void InvalidateLayout(string name, HeaderFieldDictionary fields)
        {
            throw new NotImplementedException();
        }

        public void InvalidatePage(string path, HeaderFieldDictionary fields)
        {
            throw new NotImplementedException();
        }

        public void InvalidateTemplate(string name, HeaderFieldDictionary fields)
        {
            throw new NotImplementedException();
        }
    }
}
