using System.Collections.Generic;
using System.Linq;

namespace FlipLeaf.Templating
{
    public class FormTemplate
    {
        public static readonly FormTemplate Default = CreateDefaultTemplate();

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? ContentName { get; set; }

        public string? ContentDescription { get; set; }

        public IEnumerable<FormTemplateField> Fields { get; set; } = Enumerable.Empty<FormTemplateField>();

        private static FormTemplate CreateDefaultTemplate()
        {
            return new FormTemplate
            {
                Title = "New Page",
                Description = "A standard page.",
                Fields = new List<FormTemplateField>
                {
                    new FormTemplateField
                    {
                        Id = "Title",
                        Name = "Title",
                        Description = "Title of the page, used on the browser tab",
                        Type = FormTemplateFieldType.Text,
                        DefaultValue = "New page",
                    }
                }
            };
        }
    }
}
