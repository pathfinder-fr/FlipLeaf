using System.Collections.Generic;

namespace FlipLeaf.Models
{
    public class ManageEditFormPostViewModel
    {
        public ManageEditFormPostViewModel()
        {
        }

        public string? Content { get; set; }

        public string? TemplateName { get; set; }

        public string? Comment { get; set; }

        public string? Action { get; set; }
    }
}
