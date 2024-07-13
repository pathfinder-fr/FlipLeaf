using System.ComponentModel.DataAnnotations;
using FlipLeaf.Storage;
using FlipLeaf.Website;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FlipLeaf.Pages.Manage
{
    public class EditRawModel : PageModel
    {
        private readonly IGitRepository _git;
        private readonly IWebsiteIdentity _websiteIdentity;
        private readonly IFileSystem _fileSystem;

        public EditRawModel(
            IFileSystem fileSystem,
            IGitRepository git,
            IWebsiteIdentity websiteIdentity)
        {
            _git = git;
            _websiteIdentity = websiteIdentity;
            _fileSystem = fileSystem;
            Path = string.Empty;
        }

        [BindProperty]
        public string Path { get; set; }

        [Display(Name = "Content")]
        [BindProperty]
        public string? PageContent { get; set; }

        [Display]
        [BindProperty]
        public string? Comment { get; set; }

        [BindProperty]
        public string? Action { get; set; }

        public IActionResult OnGet(string path)
        {

            var file = _fileSystem.GetItem(path);
            if (file == null)
            {
                return NotFound();
            }

            if (_fileSystem.DirectoryExists(file))
            {
                return NotFound();
            }

            var content = string.Empty;
            if (_fileSystem.FileExists(file))
            {
                // read raw content
                content = _fileSystem.ReadAllText(file);
            }

            // handle raw view
            Path = path;
            PageContent = content;

            return Page();
        }

        public IActionResult OnPost(string path)
        {
            var user = _websiteIdentity.GetCurrentUser();
            if (user == null)
            {
                return Unauthorized();
            }

            var fileItem = _fileSystem.GetItem(path);
            if (fileItem == null)
            {
                return NotFound();
            }

            _fileSystem.WriteAllText(fileItem, PageContent ?? string.Empty);

            var websiteUser = _websiteIdentity.GetWebsiteUser();
            _git.Commit(user, websiteUser, path, Comment);
            _git.PullPush(websiteUser);

            if (Action == "SaveAndContinue")
            {
                return Page();
            }
            else
            {
                return RedirectToPage("Show", new { path });
            }
        }
    }
}
