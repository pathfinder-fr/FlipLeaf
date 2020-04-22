using System.Collections.Generic;

namespace FlipLeaf.Website
{
    public interface IWebsite
    {
        IDictionary<string, Template> Templates { get; }

        IDictionary<string, Layout> Layouts { get; }
    }
}
