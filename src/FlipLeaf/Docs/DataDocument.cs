using System.Collections;
using System.Collections.Generic;
using FlipLeaf.Storage;

namespace FlipLeaf.Docs
{
    public class DataDocument : Document
    {
        public DataDocument(IStorageItem item)
            : base(item)
        {
        }
    }

    public interface IDataDictionary
    {
        object? this[string key] { get; }

        bool TryGetValue(string key, out object? value);
    }
}
