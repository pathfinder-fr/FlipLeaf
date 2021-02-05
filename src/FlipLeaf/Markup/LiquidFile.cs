using System;
using FlipLeaf.Storage;

namespace FlipLeaf.Markup
{
    public class LiquidFile
    {
        public LiquidFile(IStorageItem file)
        {
            File = file ?? throw new ArgumentNullException(nameof(file));
        }

        public virtual string Name => File.RelativePath;

        protected IStorageItem File { get; }

        public override int GetHashCode() => File.GetHashCode();

        public override bool Equals(object? obj)
        {
            if (obj is LiquidFile item)
            {
                return File.Equals(item.File);
            }

            return base.Equals(obj);
        }
    }
}
