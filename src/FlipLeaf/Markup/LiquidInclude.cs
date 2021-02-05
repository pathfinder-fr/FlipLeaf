using FlipLeaf.Storage;

namespace FlipLeaf.Markup
{
    public class LiquidInclude : LiquidFile
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
