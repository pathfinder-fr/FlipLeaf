using System;
using System.Collections.Generic;
using System.Text;

namespace FlipLeaf
{
    public class SiteSettings
    {
        private const string DefaultLayoutsFolder = "_layouts";

        public string Title { get; set; }

        public string LayoutFolder { get; set; } = DefaultLayoutsFolder;
    }
}
