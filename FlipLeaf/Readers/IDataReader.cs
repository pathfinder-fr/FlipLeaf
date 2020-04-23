using System.Collections.Generic;
using FlipLeaf.Storage;

namespace FlipLeaf.Readers
{
    public interface IDataReader
    {
        bool Accept(IStorageItem file);

        void ParseData(IStorageItem file, IDictionary<string, object> data);
    }
}
