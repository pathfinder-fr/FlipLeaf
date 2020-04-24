using System;
using System.Collections.Generic;
using System.IO;
using FlipLeaf.Readers;
using FlipLeaf.Storage;

namespace FlipLeaf.Website
{
    public class DefaultWebsite : IWebsite, IWebsiteEventHandler
    {
        private readonly IFileSystem _fileSystem;
        private readonly IEnumerable<IDataReader> _readers;

        public DefaultWebsite(IFileSystem fileSystem, IEnumerable<IDataReader> readers)
        {
            _fileSystem = fileSystem;
            _readers = readers;
        }

        public IDictionary<string, object> Data { get; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public void Populate()
        {
            // populate Data
            var dataDirItem = _fileSystem.GetItem(KnownFolders.Data);
            if (dataDirItem != null && _fileSystem.DirectoryExists(dataDirItem))
            {
                PopulateDataDirectory(dataDirItem);
            }

        }

        private void PopulateDataDirectory(IStorageItem directory)
        {
            foreach (var file in _fileSystem.GetFiles(directory))
            {
                foreach (var reader in _readers)
                {
                    if (reader.Accept(file))
                    {
                        reader.ParseData(file, Data);
                    }
                }
            }

            foreach (var subDirectory in _fileSystem.GetSubDirectories(directory))
            {
                PopulateDataDirectory(subDirectory);
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
                //_layouts[name] = new Layout(name);
            }
        }

        public void InvalidateTemplate(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            if (name != null)
            {
                //_templates[name] = new Template(name);
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

