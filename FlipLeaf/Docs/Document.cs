using System;
using FlipLeaf.Storage;

namespace FlipLeaf.Docs
{
    public class Document
    {
        public Document(IStorageItem file)
        {
            File = file ?? throw new ArgumentNullException(nameof(file));
        }

        public IStorageItem File { get; }

        public string Name => File.Name;

        public override string ToString() => File.RelativePath;
    }
}
