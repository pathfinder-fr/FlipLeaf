using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FlipLeaf.Models;
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
        private readonly Storage.IGitRepository _git;
        private readonly Storage.IFileSystem _fileSystem;
        private readonly Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider _contentTypeProvider =
            new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();

        public RenderController(
            ILogger<ManageController> logger,
            Rendering.IYamlParser yaml,
            Rendering.ILiquidRenderer liquid,
            Rendering.IMarkdownRenderer markdown,
            Storage.IGitRepository git,
            Storage.IFileSystem fileSystem
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
        public async Task<IActionResult> IndexAsync(string path)
        {
            // compute (relative)path and full path
            path ??= string.Empty;
            var fullPath = _fileSystem.GetFullPath(path);

            // default file
            // if the full path designates a directory, we change the fullpath to the default document
            if (Directory.Exists(fullPath))
            {
                fullPath = Path.Combine(fullPath, DefaultDocumentName);
                path = Path.Combine(path, DefaultDocumentName);
            }

            var ext = _fileSystem.GetExtension(fullPath);

            // .md => redirect
            // we don't want to keep .md file in url, so we redirect to the html representation
            if (string.Equals(ext, ".md", StringComparison.Ordinal))
            {
                return RedirectToAction("Index", new { path = _fileSystem.ReplaceExtension(path, ".html", ext) });
            }

            // .html => .md
            // now we want to get the source markdown file from the .html, 
            // but only if the .html itself does not exist physically on disk
            if (string.Equals(ext, ".html", StringComparison.Ordinal) && !System.IO.File.Exists(fullPath))
            {
                fullPath = _fileSystem.ReplaceExtension(fullPath, ".md", ext);
                path = _fileSystem.ReplaceExtension(path, ".md", ext);
            }

            // here we have the final file on disk
            // security check: do not manipulate files outside the base path
            if (!_fileSystem.IsPathRooted(fullPath))
            {
                return NotFound();
            }

            // if file does not exists, redirect to page creation
            if (!_fileSystem.CheckFileExists(fullPath))
            {
                return RedirectToAction("Edit", "Manage", new { path });
            }

            ext = _fileSystem.GetExtension(fullPath);

            if (ext != ".md" && ext != ".html")
            {
                if (!_contentTypeProvider.TryGetContentType(ext, out var contentType))
                {
                    contentType = "application/octet-stream";
                }

                return PhysicalFile(fullPath, contentType);
            }

            // Here start content parsing and rendering

            // 1) read all content
            var content = _fileSystem.ReadAllText(fullPath);

            // 2) parse yaml header
            var pageContext = _yaml.ParseHeader(content, out content);

            // 3) parse liquid
            content = await _liquid.RenderAsync(content, pageContext, out var context).ConfigureAwait(false);

            // 4) parse markdown (if required...)
            if (string.Equals(ext, ".md", StringComparison.Ordinal))
            {
                content = _markdown.Render(content);
            }

            // 5) apply liquid template
            content = await _liquid.ApplyLayoutAsync(content, context).ConfigureAwait(false);

            // GIT: retrieve latest commit
            var commit = _git.LogFile(_fileSystem.GetRelativePath(fullPath), 1)
                .FirstOrDefault();

            var vm = new RenderIndexViewModel
            {
                Html = content,
                Items = pageContext,
                Path = path,
                LastUpdate = commit?.Authored ?? DateTimeOffset.Now
            };

            // we automatically use the "title" yaml header as the Page Title
            if (pageContext.TryGetValue("title", out var pageTitle) && pageTitle != null)
            {
                vm.Title = pageTitle.ToString() ?? string.Empty;
                ViewData["Title"] = vm.Title;
            }

            return View(vm);
        }
    }
}
