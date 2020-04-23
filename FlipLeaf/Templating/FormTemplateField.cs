using System.Collections.Generic;

namespace FlipLeaf.Templating
{
    public class FormTemplateField
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public FormTemplateFieldType Type { get; set; }

        public FormTemplateFieldVisibility Visibility { get; set; }

        public int Cols { get; set; } = 12;

        public string? Description { get; set; }

        public object? DefaultValue { get; set; }

        public List<FormTemplateFieldChoiceItem>? Choices { get; set; }
    }

    public class FormField
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public FormTemplateFieldType Type { get; set; }

        public string? Description { get; set; }

        public object? DefaultValue { get; set; }

        public List<FormTemplateFieldChoiceItem>? Choices { get; set; }
    }
}
