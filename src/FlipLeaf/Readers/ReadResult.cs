namespace FlipLeaf.Readers
{
    public interface IReadResult
    {
    }

    public class RedirectReadResult(string url) : IReadResult
    {
        public string Url { get; } = url;
    }

    public class ContentReadResult(string content, HeaderFieldDictionary headers, string contentType) : IReadResult
    {
        public string Content { get; } = content;

        public string ContentType { get; } = contentType;

        public HeaderFieldDictionary Headers { get; } = headers;
    }
}
