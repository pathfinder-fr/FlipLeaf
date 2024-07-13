using Microsoft.Extensions.Primitives;

namespace FlipLeaf.Templating
{
    public class FormTemplateFieldChoiceItem
    {
        public string? Value { get; set; }

        public string? Text { get; set; }

        public bool IsSelected(StringValues values) => values.Any(v => Equals(Value, v));
    }
}
