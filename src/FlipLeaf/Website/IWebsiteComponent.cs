using FlipLeaf.Docs;
using FlipLeaf.Storage;

namespace FlipLeaf.Website
{
    public interface IWebsiteComponent
    {
        void OnLoad(IFileSystem fileSystem, DocumentStore docs);
    }

    public sealed class DocumentStore
    {
        public void Add(IDocument document)
        {

        }
    }
}
