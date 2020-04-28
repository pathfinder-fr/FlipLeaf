using Microsoft.AspNetCore.Mvc;

namespace FlipLeaf.Readers
{
    public interface IReadResult
    {

    }

    public class MvcActionReadResult : IReadResult
    {
        public MvcActionReadResult(IActionResult actionResult)
        {
            ActionResult = actionResult;
        }

        public IActionResult ActionResult { get; }
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
