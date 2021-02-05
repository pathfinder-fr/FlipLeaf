using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace FlipLeaf.Markup.Liquid
{
    public class FlipLeafFileProvider : IFileProvider
    {
        private readonly IDictionary<string, LiquidInclude> _includes;

        public FlipLeafFileProvider(IDictionary<string, LiquidInclude> includes)
        {
            _includes = includes;
        }

        public IDirectoryContents GetDirectoryContents(string subpath) => NotFoundDirectoryContents.Singleton;

        public IFileInfo GetFileInfo(string subpath)
        {
            if (_includes.TryGetValue(subpath, out var include))
            {
                return new IncludeFileInfo(include);
            }

            return new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter) => NullChangeToken.Singleton;

        private class IncludeFileInfo : IFileInfo
        {
            private readonly byte[] _content;

            public IncludeFileInfo(LiquidInclude include)
            {
                PhysicalPath = include.File.FullPath;
                Name = include.File.Name;
                _content = include.Content;
            }

            public bool Exists => true;

            public long Length => 0;

            public string PhysicalPath { get; }

            public string Name { get; }

            public DateTimeOffset LastModified => DateTime.MinValue;

            public bool IsDirectory => false;

            public Stream CreateReadStream() => new MemoryStream(_content);
        }
    }
}
