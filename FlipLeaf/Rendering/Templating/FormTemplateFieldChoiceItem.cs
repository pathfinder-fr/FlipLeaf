using System.Linq;
using Microsoft.Extensions.Primitives;

namespace FlipLeaf.Rendering.Templating
{
    public class FormTemplateFieldChoiceItem
    {
        public string? Value { get; set; }

        public string? Text { get; set; }

        public bool IsSelected(StringValues values) => values.Any(v => Equals(Value, v));
    }
}
