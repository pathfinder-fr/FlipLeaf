using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using FlipLeaf.Areas.Root.Models;
using FlipLeaf.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FlipLeaf.Areas.Root.Controllers
{
    [Area("Root"), Route("_manage")]
    public class ManageController : Controller
    {
        private readonly string _basePath;
        private readonly ILogger<ManageController> _logger;
        private readonly IGitService _git;
        private readonly IWebsite _website;

        public ManageController(ILogger<ManageController> logger, FlipLeafSettings settings, IGitService git, IWebsite website)
        {
            _logger = logger;
            _git = git;
            _website = website;
            _basePath = settings.SourcePath;
        }

        [Route("")]
        public IActionResult Index() => Browse(string.Empty);

        [Route("browse/{*path}")]
        public IActionResult Browse(string path)
        {
            path ??= string.Empty;
            var fullPath = Path.Combine(_basePath, path);
            if (!new Uri(fullPath).LocalPath.StartsWith(_basePath, true, CultureInfo.InvariantCulture))
            {
                return NotFound();
            }

            if (!Directory.Exists(fullPath))
            {
                return NotFound();
            }

            var vm = new ManageBrowseViewModel()
            {
                Path = path,
                PathParts = path.Split('/')
            };

            vm.Directories = Directory
                .GetDirectories(fullPath)
                .Select(f => new ManageBrowseItem { IsDirectory = true, Path = ItemPath.FromFullPath(_basePath, f) })
                .Where(f => f.Path.Name[0] != '.') // ignore all directories starting with '.'
                .ToList();

            vm.Files = Directory
                .GetFiles(fullPath, "*.*")
                .Select(f => new ManageBrowseItem { IsDirectory = false, Path = ItemPath.FromFullPath(_basePath, f) })
                .Where(f => f.Path.Name[0] != '.') // ignore all files starting with '.'
                .ToList();


            _git.SetLastCommit(path, vm.Files.ToDictionary(f => f.Path.Name, f => f), (f, date) => f.LastUpdate = date);

            return View(nameof(Browse), vm);
        }

        [Route("edit/{**path}")]
        public IActionResult Edit(string path)
        {
            path = path ?? string.Empty;
            var fullPath = Path.Combine(_basePath, path);

            if (!new Uri(fullPath).LocalPath.StartsWith(_basePath, true, CultureInfo.InvariantCulture))
            {
                return NotFound();
            }

            var vm = new ManageEditViewModel()
            {
                Path = path,
                PathParts = path.Split('/'),
            };

            if (System.IO.File.Exists(fullPath))
            {
                vm.Content = System.IO.File.ReadAllText(fullPath, Encoding.UTF8);
            }

            return View(vm);
        }

        [Route("edit/{**path}"), HttpPost]
        public IActionResult Edit(string path, ManageEditViewModel model)
        {
            var user = _website.GetCurrentUser();
            if (user == null)
            {
                return Unauthorized();
            }

            path = path ?? string.Empty;
            var fullPath = Path.Combine(_basePath, path);

            if (!new Uri(fullPath).LocalPath.StartsWith(_basePath, true, CultureInfo.InvariantCulture))
            {
                return NotFound();
            }

            var dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            System.IO.File.WriteAllText(fullPath, model.Content);

            var websiteUser = _website.GetWebsiteUser();
            _git.Commit(user, websiteUser, path, model.Comment);
            _git.PullPush(websiteUser);

            if (model.Action == "SaveAndContinue")
            {
                return RedirectToAction("Edit");
            }
            else
            {
                return RedirectToAction("Index", "Render", new { path });
            }
        }
    }
}
