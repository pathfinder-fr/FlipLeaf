namespace FlipLeaf
{
    partial class Engine
    {
        private static class Markdown
        {
            public static bool Parse(ref string source)
            {
                source = Markdig.Markdown.ToHtml(source);

                return true;
            }
        }
    }
}
