using System;
using System.Collections.Generic;
using System.Linq;
using FlipLeaf.Storage;
using FlipLeaf.Website;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FlipLeaf.Pages._Manage
{
    public class BrowseModel : PageModel
    {
        private readonly IFileSystem _fileSystem;
        private readonly IGitRepository _git;
        private readonly IWebsiteIdentity _website;

        public BrowseModel(
            IFileSystem fileSystem,
            IGitRepository git,
            IWebsiteIdentity website)
        {
            _git = git;
            _website = website;
            _fileSystem = fileSystem;
            Directories = new List<ManageBrowseItem>();
            Files = new List<ManageBrowseItem>();
        }

        public string Path { get; private set; } = string.Empty;

        public List<ManageBrowseItem> Directories { get; private set; }

        public List<ManageBrowseItem> Files { get; private set; }

        public string CombineDirectory(string directoryName)
        {
            if (string.IsNullOrEmpty(Path))
            {
                return directoryName;
            }
            else
            {
                return Path + "/" + directoryName;
            }

        }

        public string CombineFile(string fileName)
        {
            if (string.IsNullOrEmpty(Path))
            {
                return fileName;
            }
            else
            {
                return Path + "/" + fileName;
            }
        }

        public IActionResult OnGet(string path)
        {
            var directory = _fileSystem.GetItem(path);
            if (directory == null)
            {
                return NotFound();
            }

            if (!_fileSystem.DirectoryExists(directory))
            {
                return NotFound();
            }

            var directories = _fileSystem.GetSubDirectories(directory, prefixDotIncluded: false, prefixUnderscoreIncluded: true)
                .Select(f => new ManageBrowseItem(f, true))
                .OrderBy(f => f.Path.RelativePath)
                .ToList();

            var files = _fileSystem.GetFiles(directory, prefixDotIncluded: false, prefixUnderscoreIncluded: true)
                .Select(f => new ManageBrowseItem(f, false))
                .OrderBy(f => f.Path.RelativePath)
                .ToList();

            this.Path = directory.RelativePath;
            this.Directories = directories;
            this.Files = files;

            //_git.SetLastCommit(directory.RelativePath, this.Files.ToDictionary(f => f.Path.Name, f => f), (f, date) => f.WithCommit(date));

            return Page();
        }

        public IActionResult OnGetDeleteFile(string path)
        {
            var file = _fileSystem.GetItem(path);
            if (file == null)
            {
                return BadRequest();
            }

            if (_fileSystem.FileExists(file))
            {
                _git.Commit(_website.GetCurrentUser(), _website.GetWebsiteUser(), path, $"Delete {path}", remove: true);

                _fileSystem.DeleteFile(file);
            }

            var dir = _fileSystem.GetDirectoryItem(file);

            return RedirectToPage(new { path = dir.RelativePath });
        }

        public IActionResult OnPostCreateDirectory(string path, string name)
        {
            var directory = _fileSystem.GetItem(path);
            if (directory == null || !_fileSystem.DirectoryExists(directory))
            {
                return BadRequest();
            }

            var subDirectory = _fileSystem.Combine(directory, name);

            _fileSystem.EnsureDirectory(subDirectory);

            return RedirectToPage(new { path = subDirectory.RelativePath });
        }

        public IActionResult OnPostCreateFile(string path, string name)
        {
            var directory = _fileSystem.GetItem(path);
            if (directory == null || !_fileSystem.DirectoryExists(directory))
            {
                return BadRequest();
            }

            var file = _fileSystem.Combine(directory, name);

            if (_fileSystem.DirectoryExists(file))
            {
                return BadRequest("A folder with the same name already exists");
            }

            return RedirectToPage("Edit", new { path = file.RelativePath });
        }
    }

    public class ManageBrowseItem
    {
        public ManageBrowseItem(Storage.IStorageItem path, bool isDirectory)
        {
            Path = path;
            IsDirectory = isDirectory;
        }

        public bool IsDirectory { get; }

        public Storage.IStorageItem Path { get; }

        public DateTimeOffset? LastUpdate { get; set; }

        public ManageBrowseItem WithCommit(LibGit2Sharp.Commit commit)
        {
            if (commit != null)
            {
                LastUpdate = commit.Author.When;
            }

            return this;
        }
    }
}
