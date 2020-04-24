using System;
using FlipLeaf.Readers;
using FlipLeaf.Storage;

namespace FlipLeaf.Docs
{
    public class DataDocument : FileDocument
    {
        public DataDocument(IStorageItem item, IDataReader reader)
            : base(item)
        {
            Reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public IDataReader Reader { get; }
    }
}
