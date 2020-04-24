using System;
using FlipLeaf.Storage;

namespace FlipLeaf.Docs
{
    public class FileDocument : IDocument
    {
        public FileDocument(IStorageItem file)
        {
            File = file ?? throw new ArgumentNullException(nameof(file));
        }

        protected IStorageItem File { get; }

        public override int GetHashCode() => File.GetHashCode();

        public override bool Equals(object? obj)
        {
            if (obj is FileDocument item)
            {
                return File.Equals(item.File);
            }

            return base.Equals(obj);
        }
    }
}
