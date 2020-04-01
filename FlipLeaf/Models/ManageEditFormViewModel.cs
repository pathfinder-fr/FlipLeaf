using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FlipLeaf.Models
{
    public class ManageEditFormViewModel : ManageDirectoryViewModelBase
    {
        [Display]
        public string Content { get; set; }

        public IDictionary<string, object> Form { get; set; }

        public Services.FormTemplating.FormTemplate FormTemplate { get; set; }

        [Display]
        public string Comment { get; set; }

        public string Action { get; set; }
    }
}
