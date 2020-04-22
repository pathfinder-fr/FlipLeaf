using System.Collections.Generic;
using System.IO;
using System.Linq;
using FlipLeaf.Models;
using FlipLeaf.Rendering.Templating;
using FlipLeaf.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace FlipLeaf.Controllers
{
    [Route("_manage")]
    public class ManageController : Controller
    {
        private readonly ILogger<ManageController> _logger;
        private readonly Rendering.IYamlParser _yaml;
        private readonly Rendering.ILiquidRenderer _liquid;
        private readonly Rendering.IFormTemplateParser _formTemplate;
        private readonly IFileSystem _fileSystem;
        private readonly IGitRepository _git;
        private readonly Website.IWebsiteIdentity _website;

        public ManageController(
            ILogger<ManageController> logger,
            Rendering.IYamlParser yaml,
            Rendering.ILiquidRenderer liquid,
            Rendering.IFormTemplateParser formTemplate,
            IFileSystem fileSystem,
            IGitRepository git,
            Website.IWebsiteIdentity website)
        {
            _logger = logger;
            _git = git;
            _yaml = yaml;
            _liquid = liquid;
            _formTemplate = formTemplate;
            _fileSystem = fileSystem;
            _website = website;
        }

        [HttpGet("")]
        public IActionResult Index() => Browse(string.Empty);

        [HttpGet("browse/{**path}")]
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

        [HttpPost("directory/create")]
        public IActionResult CreateDirectory(string path, string name)
        {
            var directory = _fileSystem.GetItem(path);
            if (directory == null || !_fileSystem.DirectoryExists(directory))
            {
                return BadRequest();
            }

            var subDirectory = _fileSystem.Combine(directory, name);

            _fileSystem.EnsureDirectory(subDirectory);

            return RedirectToAction(nameof(Browse), new { path = subDirectory.RelativePath });
        }

        [HttpPost("file/create")]
        public IActionResult CreateFile(string path, string name)
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

            return RedirectToAction(nameof(Edit), new { path = file.RelativePath });
        }

        [HttpGet("file/delete")]
        public IActionResult DeleteFile(string path)
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

            return RedirectToAction(nameof(Browse), new { path = dir.RelativePath });
        }

        [HttpGet("edit/{**path}")]
        public IActionResult Edit(string path, bool form = false, string? template = null)
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

            if (!TryLoadTemplate(template, file, out var yamlHeader, out var templateName, out var formTemplate, out var content)
                || formTemplate == null)
            {
                if (form || template != null)
                {
                    return BadRequest("This file does not supports form editing. No template or invalid template specified");
                }

                return this.RedirectToAction(nameof(EditRaw), new { path });
            }

            var formValues = new Dictionary<string, StringValues>();
            foreach (var yamlItem in yamlHeader)
            {
                if (yamlItem.Value is string yamlString)
                {
                    formValues[yamlItem.Key] = new StringValues(yamlString);
                }
                else if (yamlItem.Value is List<object> yamlArray)
                {
                    formValues[yamlItem.Key] = new StringValues(yamlArray.Select(x => x.ToString()).ToArray());
                }
            }

            var fvm = new ManageEditFormViewModel(path)
            {
                Form = formValues,
                TemplateName = templateName,
                FormTemplate = formTemplate,
                Content = content
            };

            return View(nameof(Edit), fvm);
        }

        [HttpPost("edit/{**path}")]
        public IActionResult Edit(string path, ManageEditFormPostViewModel model)
        {
            var user = _website.GetCurrentUser();
            if (user == null)
            {
                return Unauthorized();
            }

            var file = _fileSystem.GetItem(path);
            if (file == null)
            {
                return NotFound();
            }

            if (!file.IsMarkdown() || string.IsNullOrEmpty(model.TemplateName))
            {
                return BadRequest();
            }

            if (!TryLoadTemplate(model.TemplateName, file, out var yamlHeader, out _, out var formTemplate, out var content)
                || formTemplate == null)
            {
                return BadRequest();
            }

            using (var writer = new StringWriter())
            {
                writer.WriteLine("---");
                writer.WriteLine($"{KnownFields.Template}: {model.TemplateName}");

                foreach (var field in formTemplate.Fields)
                {
                    if (field.Id == null) continue;
                    if (!Request.Form.TryGetValue($"Fields.{field.Id}", out var formValues))
                    {
                        formValues = StringValues.Empty;
                    }

                    switch (field.Type)
                    {
                        case FormTemplateFieldType.Text:
                        case FormTemplateFieldType.Choice:
                        case FormTemplateFieldType.MultiCheckBox:
                            _yaml.WriteHeaderValue(writer, field.Id, formValues, field.DefaultValue?.ToString());
                            break;
                    }
                }

                writer.WriteLine("---");

                writer.Write(model.Content);

                _fileSystem.WriteAllText(file, writer.ToString());
            }

            var websiteUser = _website.GetWebsiteUser();
            _git.Commit(user, websiteUser, path, model.Comment);
            _git.PullPush(websiteUser);

            return this.RedirectToAction(nameof(Edit), new { path });
        }

        [HttpGet("edit-raw/{**path}")]
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

        [HttpPost("edit-raw/{**path}")]
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
                return RedirectToAction(nameof(Edit));
            }
            else
            {
                return RedirectToAction(nameof(Index), "Render", new { path });
            }
        }

        private bool TryLoadTemplate(
            string? templateName,
            IStorageItem file,
            out HeaderFieldDictionary yamlHeader,
            out string? loadedTemplateName,
            out FormTemplate? formTemplate,
            out string content)
        {
            formTemplate = null;
            loadedTemplateName = null;
            content = string.Empty;

            if (_fileSystem.FileExists(file) && templateName == null)
            {
                // read raw content
                content = _fileSystem.ReadAllText(file);

                // parse YAML header
                yamlHeader = _yaml.ParseHeader(content, out content);

                if (yamlHeader.TryGetValue(KnownFields.Template, out var templateNameObj))
                {
                    templateName = templateNameObj as string;
                }
            }
            else
            {
                yamlHeader = new HeaderFieldDictionary();
            }

            if (templateName == null)
            {
                return false;
            }

            // try load template
            var templateFile = _fileSystem.GetTemplate(templateName);
            if (templateFile == null)
            {
                return false;
            }

            // parse template
            loadedTemplateName = templateName;
            formTemplate = _formTemplate.ParseTemplate(templateFile.FullPath);
            return true;
        }
    }
}
