using System.ComponentModel.DataAnnotations;
using FlipLeaf.Markup;
using FlipLeaf.Storage;
using FlipLeaf.Website;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FlipLeaf.Pages._Manage
{
    public class EditRawModel : PageModel
    {
        private readonly IGitRepository _git;
        private readonly IWebsiteIdentity _website;
        private readonly IDocumentStore _docStore;
        private readonly IFileSystem _fileSystem;
        private readonly IYamlMarkup _yaml;

        public EditRawModel(
            IFileSystem fileSystem,
            IYamlMarkup yaml,
            IGitRepository git,
            IWebsiteIdentity website,
            IDocumentStore docStore)
        {
            _git = git;
            _website = website;
            _docStore = docStore;
            _fileSystem = fileSystem;
            _yaml = yaml;
            Path = string.Empty;
        }

        public string Path { get; set; }

        [Display]
        public string? PageContent { get; set; }

        [Display]
        public string? Comment { get; set; }

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
            this.Path = path;
            this.PageContent = content;

            return Page();
        }

        public IActionResult OnPost(string path)
        {
            var user = _website.GetCurrentUser();
            if (user == null)
            {
                return Unauthorized();
            }

            var fileItem = _fileSystem.GetItem(path);
            if (fileItem == null)
            {
                return NotFound();
            }

            _fileSystem.WriteAllText(fileItem, this.PageContent ?? string.Empty);

            var websiteUser = _website.GetWebsiteUser();
            _git.Commit(user, websiteUser, path, this.Comment);
            _git.PullPush(websiteUser);

            if (this.Action == "SaveAndContinue")
            {
                return Page();
            }
            else
            {
                return RedirectToPage("Show");
            }
        }
    }
}
