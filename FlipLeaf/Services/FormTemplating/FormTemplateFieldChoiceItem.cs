namespace FlipLeaf.Services.FormTemplating
{
    public class FormTemplateFieldChoiceItem
    {
        public object Value { get; set; }
        public string Text { get; set; }

        public bool IsSelected(object value)
        {
            return (object.Equals(Value, value));
        }
    }
}