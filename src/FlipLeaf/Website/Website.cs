using System;
using System.Collections.Generic;
using FlipLeaf.Readers;
using FlipLeaf.Storage;

namespace FlipLeaf.Website
{
    public class Website : IWebsite, IWebsiteComponent
    {
        private readonly IFileSystem _fileSystem;
        private readonly IEnumerable<IDataReader> _readers;
        private readonly Dictionary<string, object> _data;

        public Website(
            IFileSystem fileSystem,
            IEnumerable<IDataReader> readers
            )
        {
            _fileSystem = fileSystem;
            _readers = readers;
            _data = new Dictionary<string, object>(StringComparer.Ordinal);
        }

        public IReadOnlyDictionary<string, object> Data => _data;

        public void OnLoad(IFileSystem fileSystem, IDocumentStore docs)
        {
            IStorageItem? dirItem;

            // populate Data
            _data.Clear();
            dirItem = _fileSystem.GetItem(KnownFolders.Data);
            if (dirItem != null && _fileSystem.DirectoryExists(dirItem))
            {
                PopulateDataDirectory(dirItem);
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
                        reader.Read(file, _data);
                    }
                }
            }

            foreach (var subDirectory in _fileSystem.GetSubDirectories(directory))
            {
                PopulateDataDirectory(subDirectory);
            }
        }
    }
}

