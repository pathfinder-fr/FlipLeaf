using System;

namespace FlipLeaf.Areas.Root.Models
{
    public class RenderIndexViewModel
    {
        public string Path { get; set; }

        public string Html { get; set; }

        public DateTimeOffset LastUpdate { get; set; }
    }
}
