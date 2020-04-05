using System.Collections.Generic;

namespace FlipLeaf.Website
{
    public interface IWebsite
    {
        IEnumerable<Template> Templates { get; }

        IEnumerable<Layout> Layouts { get; }

    }
}
