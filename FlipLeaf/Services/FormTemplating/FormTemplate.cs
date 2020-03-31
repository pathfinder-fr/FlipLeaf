using System;
using System.Collections.Generic;

namespace FlipLeaf.Services.FormTemplating
{
    public class FormTemplate
    {
        public static readonly FormTemplate Default = CreateDefaultTemplate();

        public string Title { get; set; }

        public string Description { get; set; }

        public List<FormTemplateField> Fields { get; set; }

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