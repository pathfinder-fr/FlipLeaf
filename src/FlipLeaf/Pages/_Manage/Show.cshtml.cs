using System.Collections.Generic;
using FlipLeaf.Readers;
using FlipLeaf.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FlipLeaf.Pages._Manage
{
    public class ShowModel : PageModel
    {
        private readonly IFileSystem _fileSystem;
        private readonly IEnumerable<IContentReader> _contentReaders;

        public ShowModel(
            IFileSystem fileSystem,
            IEnumerable<IContentReader> contentReaders)
        {
            _fileSystem = fileSystem;
            _contentReaders = contentReaders;
        }

        public IActionResult OnGet(string path)
        {
            var file = _fileSystem.GetItem(path);
            if (file == null)
            {
                return NotFound();
            }

            if (!_fileSystem.FileExists(file))
            {
                return NotFound();
            }

            if (file.FamilyFolder != FamilyFolder.None)
            {
                return this.Redirect(path);
            }

            foreach (var reader in _contentReaders)
            {
                if (reader.AcceptFileAsRequest(file, out var requestFile))
                {
                    return this.Redirect("/" + requestFile.RelativePath);
                }
            }

            return NotFound();
        }
    }
}
