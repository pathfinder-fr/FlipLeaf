using System;
using System.Collections.Generic;
using System.Text;

namespace FlipLeaf.Templates
{
    public class Spell
    {
        [Field(Name = "Nom", Description = "Nom du sort", Required = true)]
        public string Title { get; set; }


        [Field(Name = "Nom VO", Description = "Nom du sort en anglais")]
        public string TitleEn { get; set; }

        [Field(Name = "Source", Description = "Source de l'ouvrage", Required = true)]
        public string Source { get; set; }

        public ChoiceItem[] SourceList = new[]
        {
            new ChoiceItem("", "(Autre)"),
            new ChoiceItem("phb", "Manuel du Joueur")
        };
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FieldAttribute : Attribute
    {
        public FieldAttribute()
        {
        }

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
