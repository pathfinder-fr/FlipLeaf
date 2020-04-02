using System.ComponentModel.DataAnnotations;

namespace FlipLeaf.Models
{
    public class ManageEditRawViewModel : ManageDirectoryViewModelBase
    {
        public ManageEditRawViewModel(string path)
            : base(path)
        {
        }

        [Display]
        public string? Content { get; set; }

        [Display]
        public string? Comment { get; set; }

        public string? Action { get; set; }
    }
}
