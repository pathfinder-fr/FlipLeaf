namespace FlipLeaf.Rendering.FormTemplating
{
    public class FormTemplateFieldChoiceItem
    {
        public object? Value { get; set; }

        public string? Text { get; set; }

        public bool IsSelected(object? value) => Equals(Value, value);
    }
}
