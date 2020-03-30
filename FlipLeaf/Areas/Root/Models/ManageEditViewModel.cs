using System.ComponentModel.DataAnnotations;

namespace FlipLeaf.Areas.Root.Models
{
    public class ManageEditViewModel : ManageDirectoryViewModelBase
    {
        [Display]
        public string Content { get; set; }

        [Display]
        public string Comment { get; set; }

        public string Action { get; set; }
    }
}
