﻿using System.IO;
using FlipLeaf.Markup.Liquid;
using FlipLeaf.Storage;

namespace FlipLeaf.Markup
{
    public class LiquidLayout : LiquidFile
    {
        public LiquidLayout(IStorageItem file, HeaderFieldDictionary yamlHeader, LayoutTemplate template)
            : base(file)
        {
            this.Name = Path.GetFileNameWithoutExtension(file.Name);
            YamlHeader = yamlHeader;
            Template = template;
        }

        public override string Name { get; }

        public HeaderFieldDictionary YamlHeader { get; }

        public LayoutTemplate Template { get; }

        public override int GetHashCode() => Name.GetHashCode();

        public override string ToString() => Name;
    }
}
