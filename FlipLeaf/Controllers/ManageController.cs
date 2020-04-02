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
                .OrderBy(f => f.Path.RelativePath)
                .ToList();

            var files = _fileSystem.GetFiles(directory, prefixDotIncluded: false, prefixUnderscoreIncluded: true)
                .Select(f => new ManageBrowseItem(f, false))
                .OrderBy(f => f.Path.RelativePath)
                .ToList();

            var vm = new ManageBrowseViewModel(directory.RelativePath, directories, files);

            _git.SetLastCommit(directory.RelativePath, vm.Files.ToDictionary(f => f.Path.Name, f => f), (f, date) => f.WithCommit(date));

            return View(nameof(Browse), vm);
        }

        [Route("edit/{**path}")]
        public IActionResult Edit(string path)
        {
            var file = _fileSystem.GetItem(path);
            if (file == null)
            {
                return NotFound();
            }

            // detect templating
            var templateFile = _fileSystem.GetFileFromSameDirectoryAs(file, "template.json", true);
            if (file.IsMarkdown() && templateFile != null)
            {
                // redirect to form edit
                return this.RedirectToAction(nameof(EditForm), new { path });
            }

            return this.RedirectToAction(nameof(EditRaw), new { path });
        }

        [Route("edit-form/{**path}")]
        public IActionResult EditForm(string path)
        {
            var file = _fileSystem.GetItem(path);
            if (file == null)
            {
                return NotFound();
            }

            // form edition reserved to markdown files
            if (!file.IsMarkdown())
            {
                return this.RedirectToAction(nameof(EditRaw), new { path });
            }

            // detect template
            var templateFile = _fileSystem.GetFileFromSameDirectoryAs(file, "template.json", true);
            if (templateFile == null)
            {
                return this.RedirectToAction(nameof(EditRaw), new { path });
            }

            // load form template
            var template = _formTemplate.ParseTemplate(templateFile.FullPath);

            // read raw content
            var content = _fileSystem.ReadAllText(file);

            // parse YAML header
            var fields = _yaml.ParseHeader(content, out content);

            var fvm = new ManageEditFormViewModel(path)
            {
                Form = fields,
                FormTemplate = template,
                Content = content
            };

            return View(fvm);
        }

        [Route("edit-raw/{**path}")]
        public IActionResult EditRaw(string path)
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
            var vm = new ManageEditRawViewModel(path)
            {
                Content = content
            };

            return View(vm);
        }

        [Route("edit-raw/{**path}"), HttpPost]
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
