using FlipLeaf.Storage;

namespace FlipLeaf.Website
{
    public interface IWebsiteComponent
    {
        void OnLoad(IFileSystem fileSystem, IDocumentStore docs);
    }
}
