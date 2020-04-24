using System;
using System.IO;
using FlipLeaf.Markup.Liquid;
using FlipLeaf.Storage;

namespace FlipLeaf.Docs
{
    public class LiquidLayout : FileDocument
    {
        public LiquidLayout(IStorageItem file, HeaderFieldDictionary yamlHeader, LayoutTemplate template)
            : base(file)
        {
            this.Name = Path.GetFileNameWithoutExtension(file.Name);
            YamlHeader = yamlHeader;
            Template = template;
        }

        public string Name { get; }

        public HeaderFieldDictionary YamlHeader { get; }

        public LayoutTemplate Template { get; }

        public override string ToString() => Name;
    }
}
