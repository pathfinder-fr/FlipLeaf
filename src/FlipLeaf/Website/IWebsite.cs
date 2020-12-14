namespace FlipLeaf.Website
{
    public interface IWebsite
    {
        DocumentStore<Docs.Template> Templates { get; }
    }
}
