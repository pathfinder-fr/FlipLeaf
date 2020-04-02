using System.Linq;
using FlipLeaf.Models;
using FlipLeaf.Storage;
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
        private readonly IFileSystem _fileSystem;
        private readonly IGitRepository _git;
        private readonly IWebsite _website;

        public ManageController(
            ILogger<ManageController> logger,
            FlipLeafSettings settings,
            Rendering.IYamlParser yaml,
            Rendering.ILiquidRenderer liquid,
            Rendering.IFormTemplateParser formTemplate,
            IFileSystem fileSystem,
            IGitRepository git,
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
                .ToList();

            var files = _fileSystem.GetFiles(directory, prefixDotIncluded: false, prefixUnderscoreIncluded: true)
                .Select(f => new ManageBrowseItem(f, false))
                .ToList();

            var vm = new ManageBrowseViewModel(directory.RelativePath, directories, files);

            _git.SetLastCommit(directory.RelativePath, vm.Files.ToDictionary(f => f.Path.Name, f => f), (f, date) => f.WithCommit(date));

            return View(nameof(Browse), vm);
        }

        [Route("edit/{**path}")]
        public IActionResult Edit(string path, string mode)
        {
            mode = mode?.ToLowerInvariant() ?? string.Empty;

            var file = _fileSystem.GetItem(path);
            if (file == null)
            {
                return NotFound();
            }

            // templating
            Rendering.FormTemplating.FormTemplate? template = null;
            var templateFile = _fileSystem.GetFileFromSameDirectoryAs(file, "template.json", true);
            if (file.IsMarkdown() && templateFile != null)
            {
                template = _formTemplate.ParseTemplate(templateFile.FullPath);

                // default to form mode if a template is defined
                if (string.IsNullOrEmpty(mode))
                {
                    mode = "form";
                }
            }

            // read
            var content = _fileSystem.ReadAllText(file);

            // handle raw view
            if (mode != "form" || file.IsMarkdown())
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

        [Route("edit/raw/{**path}"), HttpPost]
        public IActionResult EditRaw(string path, ManageEditRawViewModel model)
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

            _fileSystem.WriteAllText(fileItem, model.Content ?? string.Empty);

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
