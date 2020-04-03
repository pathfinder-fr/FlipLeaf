using System;
using System.Collections.Generic;
using System.Text;

namespace FlipLeaf.Templates
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FieldAttribute : Attribute
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public bool Required { get; set; }
    }

    public class ChoiceItem
    {
        public ChoiceItem()
        {
        }

        public ChoiceItem(string value, string text)
        {
            Value = value;
            Text = text;
        }

        public ChoiceItem(string value)
        {
            Text = Value = value;
        }

        public string Value { get; set; }

        public string Text { get; set; }
    }
}
