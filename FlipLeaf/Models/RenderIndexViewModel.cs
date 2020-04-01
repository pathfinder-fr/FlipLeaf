using System;
using System.Collections.Generic;

namespace FlipLeaf.Models
{
    public class RenderIndexViewModel
    {
        public string Path { get; set; }

        public string Title { get; set; }

        public IDictionary<string, object> Items { get; set; }

        public string Html { get; set; }

        public DateTimeOffset LastUpdate { get; set; }
    }
}
