using System;
using System.IO;
using System.Linq;
using FlipLeaf.Areas.Root.Models;
using FlipLeaf.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FlipLeaf.Areas.Root.Controllers
{
    [Area("Root")]
    public class RenderController : Controller
    {
        private const string DefaultDocumentName = "index.md";
        private readonly ILogger<ManageController> _logger;
        private readonly IYamlService _yaml;
        private readonly ILiquidService _liquid;
        private readonly IMarkdownService _markdown;
        private readonly IGitService _git;
        private readonly string _basePath;

        public RenderController(
            ILogger<ManageController> logger,
            FlipLeafSettings settings,
            IYamlService yaml,
            ILiquidService liquid,
            IMarkdownService markdown,
            IGitService git)
        {
            _logger = logger;
            _yaml = yaml;
            _liquid = liquid;
            _markdown = markdown;
            _git = git;
            _basePath = settings.SourcePath;
        }

        [Route("{**path}", Order = int.MaxValue)]
        public IActionResult Index(string path)
        {
            // compute (relative)path and full path
            path ??= string.Empty;
            var fullPath = Path.Combine(_basePath, path);

            // default file
            // if the full path designates a directory, we change the fullpath to the default document
            if (Directory.Exists(fullPath))
            {
                fullPath = Path.Combine(fullPath, DefaultDocumentName);
                path = Path.Combine(path, DefaultDocumentName);
            }

            var ext = Path.GetExtension(fullPath).ToLowerInvariant();

            // .md => redirect
            // we don't want to keep .md file in url, so we redirect to the html representation
            if (string.Equals(ext, ".md", StringComparison.Ordinal))
            {
                return RedirectToAction("Index", new { path = path[0..^3] + ".html" });
            }

            // .html => .md
            // now we want to get the source markdown file from the .html, 
            // but only if the .html itself does not exist physically on disk
            if (string.Equals(ext, ".html", StringComparison.Ordinal) && !System.IO.File.Exists(fullPath))
            {
                fullPath = fullPath[0..^5] + ".md";
                path = path[0..^5] + ".md";
            }

            // here we have the final file on disk
            // security check: do not manipulate files outside the base path
            if (!new Uri(fullPath).LocalPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            // if file does not exists, redirect to page creation
            if (!System.IO.File.Exists(fullPath))
            {
                return RedirectToAction("Edit", "Manage", new { path });
            }

            ext = Path.GetExtension(fullPath).ToLowerInvariant();

            // Here start content parsing and rendering

            // 1) read all content
            var content = System.IO.File.ReadAllText(fullPath);

            // 2) parse yaml header
            content = _yaml.ParseHeader(content, out var pageContext);

            // 3) parse liquid
            content = _liquid.Parse(content, pageContext);

            // 4) parse markdown (if required...)
            if (string.Equals(ext, ".md", StringComparison.Ordinal))
            {
                content = _markdown.Parse(content);
            }

            // GIT: retrieve latest commit
            var commit = _git.LogFile(ItemPath.FromFullPath(_basePath, fullPath).RelativePath, 1)
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
                vm.Title = pageTitle.ToString();
                ViewData["Title"] = vm.Title;
            }

            return View(vm);
        }
    }
}
