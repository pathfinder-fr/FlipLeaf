using System.ComponentModel.DataAnnotations;

namespace FlipLeaf.Models
{
    public class ManageEditRawViewModel : ManageDirectoryViewModelBase
    {
        [Display]
        public string Content { get; set; }

        [Display]
        public string Comment { get; set; }

        public string Action { get; set; }
    }
}
