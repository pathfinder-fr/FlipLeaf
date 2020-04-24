using System;
using System.Collections.Generic;
using System.Text;
using FlipLeaf.Storage;

namespace FlipLeaf.Docs
{
    public class ContentDocument : FileDocument
    {
        public ContentDocument(IStorageItem file) : base(file)
        {
        }
    }
}
