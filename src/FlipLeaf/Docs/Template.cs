using System.IO;
using FlipLeaf.Storage;

namespace FlipLeaf.Docs
{
    public class Template : FileDocument
    {
        public Template(IStorageItem file)
            : base(file)
        {
            this.Name = Path.GetFileNameWithoutExtension(file.Name);
        }

        public string Name { get; }
    }
}
