using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using FlipLeaf.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FlipLeaf.Controllers
{
    [Route("_manage")]
    public class ManageController : Controller
    {
        private readonly string _basePath;
        private readonly ILogger<ManageController> _logger;
        private readonly Rendering.IYamlParser _yaml;
        private readonly Rendering.ILiquidRenderer _liquid;
        private readonly Rendering.IFormTemplateParser _formTemplate;
        private readonly Storage.IFileSystem _fileSystem;
        private readonly Storage.IGitRepository _git;
        private readonly IWebsite _website;

        public ManageController(
            ILogger<ManageController> logger,
            FlipLeafSettings settings,
            Rendering.IYamlParser yaml,
            Rendering.ILiquidRenderer liquid,
            Rendering.IFormTemplateParser formTemplate,
            Storage.IFileSystem fileSystem,
            Storage.IGitRepository git,
            IWebsite website)
        {
            _logger = logger;
            _git = git;
            _yaml = yaml;
            _liquid = liquid;
            _formTemplate = formTemplate;
            _fileSystem = fileSystem;
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

            var directories = Directory
                .GetDirectories(fullPath)
                .Select(f => new ManageBrowseItem(_fileSystem.ItemPathFromFullPath(f), true))
                .Where(f => f.Path.Name[0] != '.') // ignore all directories starting with '.'
                .ToList();

            var files = Directory
                .GetFiles(fullPath, "*.*")
                .Select(f => new ManageBrowseItem(_fileSystem.ItemPathFromFullPath(f), false))
                .Where(f => f.Path.Name[0] != '.') // ignore all files starting with '.'
                .ToList();


            var vm = new ManageBrowseViewModel(path, directories, files);

            _git.SetLastCommit(path, vm.Files.ToDictionary(f => f.Path.Name, f => f), (f, date) => f.WithCommit(date));

            return View(nameof(Browse), vm);
        }

        [Route("edit/{**path}")]
        public IActionResult Edit(string path, string mode)
        {
            mode = mode?.ToLowerInvariant() ?? string.Empty;
            path ??= string.Empty;
            var fullPath = Path.Combine(_basePath, path);

            if (!new Uri(fullPath).LocalPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            var dirPath = _fileSystem.GetDirectoryName(fullPath);
            var ext = _fileSystem.GetExtension(fullPath);

            // templating
            Rendering.FormTemplating.FormTemplate? template = null;
            if (dirPath != null)
            {
                var templatePath = Path.Combine(dirPath, "template.json");
                if (ext == ".md" && _fileSystem.CheckFileExists(templatePath))
                {
                    template = _formTemplate.ParseTemplate(templatePath);

                    // default to form mode if a template is defined
                    if (string.IsNullOrEmpty(mode))
                    {
                        mode = "form";
                    }
                }
            }

            // read
            var content = string.Empty;
            if (_fileSystem.CheckFileExists(fullPath))
            {
                content = _fileSystem.ReadAllText(fullPath);
            }

            // handle raw view
            if (mode != "form" || ext != ".md")
            {
                var vm = new ManageEditRawViewModel(path)
                {
                    Content = content
                };

                return View("EditRaw", vm);
            }

            // handle form view
            template ??= Rendering.FormTemplating.FormTemplate.Default;

            // parse YAML header
            var fields = _yaml.ParseHeader(content, out content);

            var fvm = new ManageEditFormViewModel(path)
            {
                Form = fields,
                FormTemplate = template,
                Content = content
            };

            return View("EditForm", fvm);
        }

        [Route("edit/{**path}"), HttpPost]
        public IActionResult EditRaw(string path, ManageEditRawViewModel model)
        {
            var user = _website.GetCurrentUser();
            if (user == null)
            {
                return Unauthorized();
            }

            path ??= string.Empty;
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
