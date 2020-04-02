using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FlipLeaf.Models
{
    public class ManageEditFormViewModel : ManageDirectoryViewModelBase
    {
        public ManageEditFormViewModel(string path)
            : base(path)
        {

        }

        [Display]
        public string? Content { get; set; }

        public IDictionary<string, object> Form { get; set; } = new Dictionary<string, object>();

        public Rendering.FormTemplating.FormTemplate FormTemplate { get; set; } = Rendering.FormTemplating.FormTemplate.Default;

        [Display]
        public string? Comment { get; set; }

        public string? Action { get; set; }
    }
}
