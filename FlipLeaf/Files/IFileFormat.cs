using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FlipLeaf.Storage;

namespace FlipLeaf.Files
{
    public interface IFileFormat
    {
        string Extension { get; }

        bool RawAllowed => true;

        Task<ParsedFile> RenderAsync(IStorageItem file);
    }
}
