using System;
using System.Collections.Generic;
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
        private readonly IYamlService _yaml;
        private readonly ILiquidService _liquid;
        private readonly IFormTemplateService _formTemplate;
        private readonly IGitService _git;
        private readonly IWebsite _website;

        public ManageController(
            ILogger<ManageController> logger,
            FlipLeafSettings settings,
            IGitService git,
            IYamlService yaml,
            ILiquidService liquid,
            IFormTemplateService formTemplate,
            IWebsite website)
        {
            _logger = logger;
            _git = git;
            _yaml = yaml;
            _liquid = liquid;
            _formTemplate = formTemplate;
            _website = website;
            _basePath = settings.SourcePath;
        }

        [Route("")]
        public IActionResult Index() => Browse(string.Empty);

        [Route("browse/{*path}")]
        public IActionResult Browse(string path)
        {
            throw new ApplicationException();

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
        public IActionResult Edit(string path, string mode)
        {
            mode = mode?.ToLowerInvariant() ?? string.Empty;
            path = path ?? string.Empty;
            var fullPath = Path.Combine(_basePath, path);

            if (!new Uri(fullPath).LocalPath.StartsWith(_basePath, true, CultureInfo.InvariantCulture))
            {
                return NotFound();
            }

            var pathParts = path.Split('/');

            var content = string.Empty;
            if (System.IO.File.Exists(fullPath))
            {
                content = System.IO.File.ReadAllText(fullPath, Encoding.UTF8);
            }

            Services.FormTemplating.FormTemplate template = null;
            var dirPath = Path.GetDirectoryName(fullPath);
            var ext = Path.GetExtension(fullPath).ToLowerInvariant();

            var templatePath = Path.Combine(dirPath, "template.json");
            if (ext == ".md" && System.IO.File.Exists(templatePath))
            {
                template = _formTemplate.ParseTemplate(templatePath);

                // default to form mode if a template is defined
                if (string.IsNullOrEmpty(mode))
                {
                    mode = "form";
                }
            }

            // handle form view
            if (ext == ".md" && mode == "form")
            {
                template = template ?? Services.FormTemplating.FormTemplate.Default;

                // parse YAML header
                content = _yaml.ParseHeader(content, out var form);

                var fvm = new ManageEditFormViewModel()
                {
                    Path = path,
                    PathParts = pathParts,
                    Form = form,
                    FormTemplate = template,
                    Content = content
                };

                return View("EditForm", fvm);
            }

            var vm = new ManageEditRawViewModel()
            {
                Path = path,
                PathParts = pathParts,
                Content = content
            };

            return View("EditRaw", vm);
        }

        [Route("edit/{**path}"), HttpPost]
        public IActionResult EditRaw(string path, ManageEditRawViewModel model)
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
