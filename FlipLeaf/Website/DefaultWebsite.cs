using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.TagHelpers;

namespace FlipLeaf.Website
{
    public class DefaultWebsite : IWebsite, IWebsiteEventHandler
    {
        private readonly ConcurrentDictionary<string, Template> _templates = new ConcurrentDictionary<string, Template>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, Layout> _layouts = new ConcurrentDictionary<string, Layout>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, Page> _pages = new ConcurrentDictionary<string, Page>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, object> _data = new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        private readonly FlipLeafSettings _settings;

        public DefaultWebsite(FlipLeafSettings settings)
        {
            _settings = settings;
        }

        public IDictionary<string, Template> Templates => _templates;

        public IDictionary<string, Layout> Layouts => _layouts;

        public IEnumerable<Page> Pages => _pages.Values;

        public IDictionary<string, object> Data => _data;

        public void Populate(string sourcePath)
        {
            foreach (var directory in Directory.GetDirectories(sourcePath))
            {
                switch (Path.GetFileName(directory))
                {
                    case KnownFolders.Layouts:
                        foreach (var file in Directory.GetFiles(directory))
                        {
                            InvalidateLayout(file);
                        }
                        break;

                    case KnownFolders.Templates:
                        foreach (var file in Directory.GetFiles(directory))
                        {
                            InvalidateTemplate(file);
                        }
                        break;

                    case KnownFolders.Data:
                        PopulateSpecialDirectory(directory, InvalidateData);
                        break;
                }
            }
        }

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
                    case KnownFolders.Templates:
                    case KnownFolders.Layouts:
                        break;
                }
            }
            else
            {
                // standard page
            }
        }

        public void PopulateSpecialDirectory(string path, Action<string> invalidateFile)
        {
            foreach (var file in Directory.GetFiles(path))
            {
                invalidateFile(file);
            }

            foreach (var directory in Directory.GetDirectories(path))
            {
                PopulateSpecialDirectory(directory, invalidateFile);
            }
        }

        public void InvalidateLayout(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            if (name != null)
            {
                _layouts[name] = new Layout(name);
            }
        }

        public void InvalidateTemplate(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            if (name != null)
            {
                _templates[name] = new Template(name);
            }
        }

        public void InvalidateData(string path)
        {
            var ext = Path.GetExtension(path);
            switch (ext)
            {
                case ".json":
                    InvalidateDataJson(path);
                    break;
            }
        }

        private void InvalidateDataJson(string path)
        {
        }
    }
}

