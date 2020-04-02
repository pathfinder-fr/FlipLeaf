using System;
using System.Collections.Generic;

namespace FlipLeaf.Models
{
    public class RenderIndexViewModel
    {
        public string Path { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public IDictionary<string, object>? Items { get; set; }

        public string Html { get; set; } = string.Empty;

        public DateTimeOffset LastUpdate { get; set; }
    }
}
