using System;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FlipLeaf.Areas.Root.Models;
using FlipLeaf.Services;
using System.Linq;

namespace FlipLeaf.Areas.Root.Controllers
{
    [Area("Root")]
    public class RenderController : Controller
    {
        private const string DefaultDocumentName = "index.md";
        private readonly ILogger<ManageController> _logger;
        private readonly ILiquidService _liquid;
        private readonly IMarkdownService _markdown;
        private readonly IYamlService _yaml;
        private readonly IGitService _git;
        private readonly string _basePath;

        public RenderController(
            ILogger<ManageController> logger,
            FlipLeafSettings settings,
            ILiquidService liquid,
            IMarkdownService markdown,
            IYamlService yaml,
            IGitService git)
        {
            _logger = logger;
            _liquid = liquid;
            _markdown = markdown;
            _yaml = yaml;
            _git = git;
            _basePath = settings.SourcePath;
        }

        [Route("{**path}", Order = int.MaxValue)]
        public IActionResult Index(string path)
        {
            path ??= string.Empty;

            // full absolute path
            var fullPath = Path.Combine(_basePath, path);

            // default file
            if (Directory.Exists(fullPath))
            {
                fullPath = Path.Combine(fullPath, DefaultDocumentName);
                path = Path.Combine(path, DefaultDocumentName);
            }

            var ext = Path.GetExtension(fullPath).ToLowerInvariant();

            // .md => redirect
            if (string.Equals(ext, ".md", StringComparison.Ordinal))
            {
                return RedirectToAction("Index", new { path = path[0..^3] + ".html" });
            }

            // .html => .md
            if (string.Equals(ext, ".html", StringComparison.Ordinal))
            {
                if (!System.IO.File.Exists(fullPath))
                {
                    fullPath = fullPath[0..^5] + ".md";
                    path = path[0..^5] + ".md";
                }
            }

            // security check, do not manipulate files outside the base path
            if (!new Uri(fullPath).LocalPath.StartsWith(_basePath, true, CultureInfo.InvariantCulture))
            {
                return NotFound();
            }

            // if file does not exists, redirect to page creation
            if (!System.IO.File.Exists(fullPath))
            {
                return RedirectToAction("Edit", "Manage", new { path });
            }

            // 1) read all content
            var content = System.IO.File.ReadAllText(fullPath);

            // 2) parse yaml header
            content = _yaml.ParseHeader(content, out var pageContext);

            // 3) parse liquid
            content = _liquid.Parse(content, pageContext);

            // 4) parse markdown
            if (string.Equals(ext, ".md", StringComparison.Ordinal))
            {
                content = _markdown.Parse(content);
            }

            var commit = _git.LogFile(ItemPath.FromFullPath(_basePath, fullPath).RelativePath).FirstOrDefault();

            var vm = new RenderIndexViewModel
            {
                Html = content,
                Path = path,
                LastUpdate = commit?.Authored ?? DateTimeOffset.Now
            };

            return View(vm);
        }
    }
}
