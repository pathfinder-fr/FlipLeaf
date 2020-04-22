using System;
using System.Linq;
using System.Threading.Tasks;
using FlipLeaf.Models;
using FlipLeaf.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FlipLeaf.Controllers
{
    public class RenderController : Controller
    {
        private const string DefaultDocumentName = "index.html";

        private readonly ILogger<ManageController> _logger;
        private readonly Rendering.IYamlParser _yaml;
        private readonly Rendering.ILiquidRenderer _liquid;
        private readonly Rendering.IMarkdownRenderer _markdown;
        private readonly IGitRepository _git;
        private readonly IFileSystem _fileSystem;
        private readonly Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider _contentTypeProvider =
            new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();

        private readonly Files.IFileFormat[] _fileFormats;

        public RenderController(
            ILogger<ManageController> logger,
            Rendering.IYamlParser yaml,
            Rendering.ILiquidRenderer liquid,
            Rendering.IMarkdownRenderer markdown,
            IGitRepository git,
            IFileSystem fileSystem
            )
        {
            _logger = logger;
            _yaml = yaml;
            _liquid = liquid;
            _markdown = markdown;
            _git = git;
            _fileSystem = fileSystem;
            _fileFormats = new Files.IFileFormat[]
            {
                new Files.HtmlFile(yaml, liquid, fileSystem),
                new    Files.MarkdownFile(yaml, liquid, markdown, fileSystem)
            };
        }

        [Route("{**path}", Order = int.MaxValue)]
        public async Task<IActionResult> Index(string path)
        {
            var file = _fileSystem.GetItem(path);
            if (file == null)
            {
                return NotFound();
            }

            if (file.RelativePath.StartsWith('.') || file.RelativePath.StartsWith('_'))
            {
                return Redirect("/");
            }

            // default file
            // if the full path designates a directory, we change the fullpath to the default document
            if (_fileSystem.DirectoryExists(file))
            {
                file = _fileSystem.Combine(file, DefaultDocumentName);
            }

            Files.IFileFormat? diskFileFormat = null;
            IStorageItem? diskFile = null;

            if (file.IsHtml())
            {
                foreach (var format in _fileFormats)
                {
                    diskFile = _fileSystem.ReplaceExtension(file, format.Extension);
                    if (_fileSystem.FileExists(diskFile))
                    {
                        diskFileFormat = format;
                        break;
                    }
                }
            }
            else
            {
                if (!_fileSystem.FileExists(file))
                {
                    return RedirectToAction(nameof(ManageController.Edit), "Manage", new { path });
                }

                foreach (var format in _fileFormats.Where(f => f.RawAllowed))
                {
                    diskFile = _fileSystem.ReplaceExtension(file, format.Extension);
                    if (_fileSystem.FileExists(diskFile))
                    {
                        diskFileFormat = format;
                        break;
                    }
                }
            }

            if (diskFileFormat == null || diskFile == null)
            {
                if (diskFile == null)
                {
                    return NotFound();
                }

                // if the file is a static resource, eg. not a markdown or html file we just return the content
                // we try to detect the content-type based on the extension
                if (!_contentTypeProvider.TryGetContentType(file.Extension, out var staticContentType))
                {
                    staticContentType = "application/octet-stream";
                }

                return PhysicalFile(diskFile.FullPath, staticContentType);
            }

            // if (file.IsMarkdown() || file.IsHtml() || file.IsJson() || file.IsXml() || file.IsYaml())
            var parsedFile = await diskFileFormat.RenderAsync(diskFile);

            // GIT: retrieve latest commit
            var commit = _git.LogFile(diskFile.RelativePath, 1).FirstOrDefault();

            if (parsedFile.ContentType == "text/html")
            {
                var vm = new RenderIndexViewModel
                {
                    Html = parsedFile.Content,
                    Items = parsedFile.Headers,
                    Path = diskFile.RelativePath,
                    ManagePath = _fileSystem.GetDirectoryItem(diskFile).RelativePath,
                    LastUpdate = commit?.Authored ?? DateTimeOffset.Now
                };

                // we automatically use the "title" yaml header as the Page Title
                if (parsedFile.Headers.TryGetValue(KnownFields.Title, out var pageTitle) && pageTitle != null)
                {
                    vm.Title = pageTitle.ToString() ?? string.Empty;
                    ViewData["Title"] = vm.Title;
                }

                return View(nameof(Index), vm);
            }

            var contentType = parsedFile.ContentType;

            // explicit content type found in headers
            if (parsedFile.Headers.TryGetValue(KnownFields.ContentType, out var contentTypeObj) && contentTypeObj is string headerContentType)
            {
                contentType = headerContentType;
            }

            // if no content-type found, try to detect it based in query path extension
            // fallback to text/plain
            if (string.IsNullOrEmpty(contentType))
            {
                if (!_contentTypeProvider.TryGetContentType(file.Extension, out contentType))
                {
                    contentType = "text/plain";
                }
            }

            return Content(parsedFile.Content, contentType);
        }
    }
}
