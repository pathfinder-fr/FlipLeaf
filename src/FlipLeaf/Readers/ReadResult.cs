namespace FlipLeaf.Readers
{
    public interface IReadResult
    {
    }

    public class RedirectReadResult : IReadResult
    {
        public RedirectReadResult(string url)
        {
            this.Url = url;
        }

        public string Url { get; }
    }

    public class ContentReadResult : IReadResult
    {
        public ContentReadResult(string content, HeaderFieldDictionary headers, string contentType)
        {
            Content = content;
            Headers = headers;
            ContentType = contentType;
        }

        public string Content { get; }

        public string ContentType { get; }

        public HeaderFieldDictionary Headers { get; }
    }
}
