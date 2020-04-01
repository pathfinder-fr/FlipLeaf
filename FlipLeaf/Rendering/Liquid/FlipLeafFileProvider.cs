using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace FlipLeaf.Rendering.Liquid
{
    public class FlipLeafFileProvider : IFileProvider
    {
        private readonly string _sourcePath;

        public FlipLeafFileProvider(FlipLeafSettings settings)
        {
            _sourcePath = settings.SourcePath;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return NotFoundDirectoryContents.Singleton;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            /*if (subpath.EndsWith(".liquid"))
            {
                subpath = subpath.Substring(0, subpath.Length - ".liquid".Length);
            }*/

            var fullPath = Path.Combine(_sourcePath, "_includes", subpath);

            if (File.Exists(fullPath))
                return new IncludeFileInfo(fullPath);

            return new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }

        private class IncludeFileInfo : IFileInfo
        {
            private readonly string _path;
            private readonly FileInfo _info;

            public IncludeFileInfo(string path)
            {
                _path = path;
                _info = new FileInfo(path);
            }

            public bool Exists => File.Exists(_path);

            public long Length => _info.Length;

            public string PhysicalPath => _path;

            public string Name => _info.Name;

            public DateTimeOffset LastModified => _info.LastWriteTime;

            public bool IsDirectory => false;

            public Stream CreateReadStream() => _info.OpenRead();
        }
    }
}
