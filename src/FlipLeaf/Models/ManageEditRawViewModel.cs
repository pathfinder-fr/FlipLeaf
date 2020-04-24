using System.ComponentModel.DataAnnotations;

namespace FlipLeaf.Models
{
    public class ManageEditRawViewModel
    {
        public ManageEditRawViewModel()
            : this(string.Empty)
        {
        }

        public ManageEditRawViewModel(string path)
        {
            this.Path = path;
        }

        public string Path { get; set; }

        [Display]
        public string? Content { get; set; }

        [Display]
        public string? Comment { get; set; }

        public string? Action { get; set; }
    }
}
