namespace FlipLeaf.Rendering
{
    public class PageRender
    {
        public PageRender()
        {
        }

        public PageRender(PageRenderResult result)
        {
            Result = result;
        }

        public PageRenderResult Result { get; set; }

        public string RedirectUrl { get; set; }

        public string PhysicalPath { get; set; }

        public string Content { get; set; }

        public string ContentType { get; set; }

        public string Html { get; set; }

        public HeaderFieldDictionary Items { get; set; }

        public string Path { get; set; }

        public string ManagePath { get; set; }

        public DateTimeOffset LastUpdate { get; set; }

        public string Title { get; set; }
    }

    public enum PageRenderResult
    {
        NotFound,

        Redirect,

        PhysicalFile,

        Page,

        Content
    }
}
