using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Primitives;

namespace FlipLeaf.Models
{
    public class ManageEditFormViewModel
    {
        public ManageEditFormViewModel()
            : this(string.Empty)
        {
        }

        public ManageEditFormViewModel(string path)
        {
            this.Path = path;
        }

        public string Path { get; set; }

        [Display]
        public string? Content { get; set; }

        public string? TemplateName { get; set; }

        public IDictionary<string, StringValues> Form { get; set; } = new Dictionary<string, StringValues>();

        public Rendering.Templating.FormTemplate FormTemplate { get; set; } = Rendering.Templating.FormTemplate.Default;

        [Display]
        public string? Comment { get; set; }

        public string? Action { get; set; }
    }
}
