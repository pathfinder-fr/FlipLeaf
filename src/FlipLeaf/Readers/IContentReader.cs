using System.Threading.Tasks;
using FlipLeaf.Storage;

namespace FlipLeaf.Readers
{
    public interface IContentReader
    {
        bool AcceptForRequest(IStorageItem diskfile, out IStorageItem requestFile);

        bool AcceptAsRequest(IStorageItem requestFile, out IStorageItem diskFile);

        Task<HeaderFieldDictionary?> ReadHeaderAsync(IStorageItem file);

        Task<ReadResult> ReadAsync(IStorageItem file);
    }
}
