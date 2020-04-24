using System;
using System.Collections.Generic;
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
        private readonly ILogger<ManageController> _logger;
        private readonly IGitRepository _git;
        private readonly IFileSystem _fileSystem;
        private readonly Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider _contentTypeProvider =
            new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();

        private readonly IEnumerable<Readers.IContentReader> _readers;

        public RenderController(
            ILogger<ManageController> logger,
            IEnumerable<Readers.IContentReader> readers,
            IGitRepository git,
            IFileSystem fileSystem
            )
        {
            _logger = logger;
            _readers = readers.ToArray();
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
                file = _fileSystem.Combine(file, KnownFiles.DefaultDocument);
            }

            // try to determine if a reader accepts this request
            Readers.IContentReader? diskFileReader = null;
            IStorageItem? diskFile = null;
            foreach (var reader in _readers)
            {
                if (reader.AcceptAsRequest(file, out diskFile))
                {
                    diskFileReader = reader;
                    break;
                }
            }

            // if there is no reader accepting this file...
            if (diskFileReader == null || diskFile == null)
            {
                // if the file is HTML we redirect to the editor (if enabled)
                if (file.IsHtml())
                {
                    return RedirectToAction(nameof(ManageController.Edit), "Manage", new { path });
                }

                // if the file does not exists, 404
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

            // ok, now we engage the reader
            var parsedFile = await diskFileReader.ReadAsync(diskFile);

            // git: retrieve latest commit
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
