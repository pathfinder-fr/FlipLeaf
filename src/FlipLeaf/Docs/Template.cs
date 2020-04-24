using System.IO;
using FlipLeaf.Storage;
using FlipLeaf.Templating;

namespace FlipLeaf.Docs
{
    public class Template : FileDocument
    {
        public Template(IStorageItem file, FormTemplate template)
            : base(file)
        {
            this.Name = Path.GetFileNameWithoutExtension(file.Name).ToLowerInvariant();
            this.FormTemplate = template;
        }

        public override string Name { get; }

        public FormTemplate FormTemplate { get; }
    }
}
