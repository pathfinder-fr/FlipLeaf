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
        private const string DefaultDocumentName = "index.md";
        private readonly ILogger<ManageController> _logger;
        private readonly Rendering.IYamlParser _yaml;
        private readonly Rendering.ILiquidRenderer _liquid;
        private readonly Rendering.IMarkdownRenderer _markdown;
        private readonly IGitRepository _git;
        private readonly IFileSystem _fileSystem;
        private readonly Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider _contentTypeProvider =
            new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();

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

            // .md => redirect
            // we don't want to keep .md file in url, so we redirect to the html representation
            if (file.IsMarkdown())
            {
                return RedirectToAction(nameof(Index), new { path = _fileSystem.ReplaceExtension(file, ".html").RelativePath });
            }

            // .html => .md
            // now we want to get the source markdown file from the .html, 
            // but only if the .html itself does not exist physically on disk
            if (file.IsHtml() && !_fileSystem.FileExists(file))
            {
                file = _fileSystem.ReplaceExtension(file, ".md");
            }

            // if file does not exists, redirect to page creation
            if (!_fileSystem.FileExists(file))
            {
                return RedirectToAction(nameof(ManageController.Edit), "Manage", new { path });
            }

            if (file.IsMarkdown() || file.IsHtml())
            {
                return await RenderParsedContent(file);
            }

            // if the file is a static resource, eg. not a markdown or html file we just return the content
            // we try to detect the content-type based on the
            if (!_contentTypeProvider.TryGetContentType(file.Extension, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return PhysicalFile(file.FullPath, contentType);
        }

        private async Task<IActionResult> RenderParsedContent(IStorageItem file)
        {
            // 1) read all content
            var content = _fileSystem.ReadAllText(file);

            // 2) parse yaml header
            var yamlHeader = _yaml.ParseHeader(content, out content);

            // 3) parse liquid
            content = await _liquid.RenderAsync(content, yamlHeader, out var context).ConfigureAwait(false);

            // 4) parse markdown (if required...)
            if (file.IsMarkdown())
            {
                content = _markdown.Render(content);
            }

            // 5) apply liquid layout
            // this call can be recusrive if there are multiple layouts
            content = await _liquid.ApplyLayoutAsync(content, context).ConfigureAwait(false);

            // GIT: retrieve latest commit
            var commit = _git.LogFile(file.RelativePath, 1).FirstOrDefault();

            var vm = new RenderIndexViewModel
            {
                Html = content,
                Items = yamlHeader,
                Path = file.RelativePath,
                LastUpdate = commit?.Authored ?? DateTimeOffset.Now
            };

            // we automatically use the "title" yaml header as the Page Title
            if (yamlHeader.TryGetValue("title", out var pageTitle) && pageTitle != null)
            {
                vm.Title = pageTitle.ToString() ?? string.Empty;
                ViewData["Title"] = vm.Title;
            }

            return View(nameof(Index), vm);
        }
    }
}
