using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace FlipLeaf.Markup.Liquid
{
    public class FlipLeafFileProvider : IFileProvider
    {
        private readonly string _sourcePath;

        public FlipLeafFileProvider(FlipLeafSettings settings)
        {
            _sourcePath = settings.SourcePath;
        }

        public IDirectoryContents GetDirectoryContents(string subpath) => NotFoundDirectoryContents.Singleton;

        public IFileInfo GetFileInfo(string subpath)
        {
            var fullPath = Path.Combine(_sourcePath, "_includes", subpath);

            if (File.Exists(fullPath))
            {
                return new IncludeFileInfo(fullPath);
            }

            return new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter) => NullChangeToken.Singleton;

        private class IncludeFileInfo : IFileInfo
        {
            private readonly FileInfo _info;

            public IncludeFileInfo(string path)
            {
                PhysicalPath = path;
                _info = new FileInfo(path);
            }

            public bool Exists => File.Exists(PhysicalPath);

            public long Length => _info.Length;

            public string PhysicalPath { get; }

            public string Name => _info.Name;

            public DateTimeOffset LastModified => _info.LastWriteTime;

            public bool IsDirectory => false;

            public Stream CreateReadStream() => _info.OpenRead();
        }
    }
}
