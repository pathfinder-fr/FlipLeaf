using System.Threading.Tasks;
using FlipLeaf.Storage;

namespace FlipLeaf.Readers
{
    public interface IContentReader
    {
        /// <summary>
        /// Checks if this reader accepts to render a disk file designated by <paramref name="diskfile"/>,
        /// and if <see langword="true"/>, returns the file <paramref name="requestFile"/> that should be used as a request.
        /// </summary>
        /// <remarks>
        /// This method is symetric with <see cref="AcceptRequest"/> :
        /// if this methods returns <see langword="true"/>, then it is guaranted that calling <see cref="AcceptRequest"/>
        /// with <paramref name="requestFile"/> will also returns <see langword="true"/>.
        /// </remarks>
        /// <param name="diskfile">File on disk.</param>
        /// <param name="requestFile">Request to use to render the file designated by <paramref name="diskfile"/>.</param>
        /// <returns><see langword="true"/> if the disk file can be rendered by this reader, <see langword="false" /> otherwise.</returns>
        bool AcceptFileAsRequest(IStorageItem diskfile, out IStorageItem requestFile);

        bool AcceptRequest(IStorageItem requestFile, out IStorageItem diskFile);

        Task<HeaderFieldDictionary?> ReadHeaderAsync(IStorageItem file);

        Task<IReadResult> ReadAsync(IStorageItem file);
    }
}
