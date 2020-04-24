using FlipLeaf.Storage;

namespace FlipLeaf.Docs
{
    public class LiquidInclude : FileDocument
    {
        public LiquidInclude(IStorageItem file, byte[] content)
            : base(file)
        {
            Content = content;
        }

        public new IStorageItem File => base.File;

        public byte[] Content { get; }
    }
}
