using System.Threading.Tasks;
using FlipLeaf.Storage;

namespace FlipLeaf.Readers
{
    public interface IContentReader
    {
        bool AcceptInverse(IStorageItem diskfile, out IStorageItem requestFile);

        bool Accept(IStorageItem requestFile, out IStorageItem diskFile);

        Task<HeaderFieldDictionary?> ReadHeaderAsync(IStorageItem file);

        Task<ReadResult> ReadAsync(IStorageItem file);
    }
}
