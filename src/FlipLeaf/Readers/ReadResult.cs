namespace FlipLeaf.Readers
{
    public class ReadResult
    {
        public ReadResult(string content, HeaderFieldDictionary headers, string contentType)
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
