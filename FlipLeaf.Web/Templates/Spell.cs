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
}
