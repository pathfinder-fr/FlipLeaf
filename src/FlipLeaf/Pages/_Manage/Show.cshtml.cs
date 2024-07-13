using FlipLeaf.Readers;
using FlipLeaf.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FlipLeaf.Pages.Manage
{
    public class ShowModel : PageModel
    {
        private readonly IFileSystem _fileSystem;
        private readonly IEnumerable<IContentReader> _contentReaders;
        private readonly FlipLeafSettings _settings;

        public ShowModel(
            IFileSystem fileSystem,
            IEnumerable<IContentReader> contentReaders,
            FlipLeafSettings settings
            )
        {
            _fileSystem = fileSystem;
            _contentReaders = contentReaders;
            _settings = settings;
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
                return Redirect(path);
            }

            foreach (var reader in _contentReaders)
            {
                if (reader.AcceptFileAsRequest(file, out var requestFile))
                {
                    return Redirect($"{_settings.BaseUrl}/" + requestFile.RelativePath);
                }
            }

            return NotFound();
        }
    }
}
