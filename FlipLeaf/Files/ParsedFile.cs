namespace FlipLeaf.Files
{
    public class ParsedFile
    {
        public ParsedFile(string content, HeaderFieldDictionary headers, string contentType)
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
