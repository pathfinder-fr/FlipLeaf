using FlipLeaf.Storage;

namespace FlipLeaf.Rendering
{
    public class PageRenderer
    {
        private readonly Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider _contentTypeProvider = new();
        private readonly IEnumerable<Readers.IContentReader> _readers;
        private readonly IGitRepository _git;
        private readonly IFileSystem _fileSystem;
        private readonly FlipLeafSettings _settings;

        public PageRenderer(
            IEnumerable<Readers.IContentReader> readers,
            IGitRepository git,
            IFileSystem fileSystem,
            FlipLeafSettings settings)
        {
            _readers = readers;
            _git = git;
            _fileSystem = fileSystem;
            _settings = settings;
        }

        public async Task<PageRender> RenderPage(string path)
        {                
            var file = _fileSystem.GetItem(path);
            if (file == null)
            {
                return new PageRender(PageRenderResult.NotFound);
            }

            // browsing dot and underscore prefixed directories is not allowed
            if (file.RelativePath.StartsWith('.') || file.RelativePath.StartsWith('_'))
            {
                return new PageRender(PageRenderResult.NotFound);
            }

            // default file
            // if the full path designates a directory, we change the path to the default document
            var isDefaultFile = false;
            if (_fileSystem.DirectoryExists(file))
            {
                file = _fileSystem.Combine(file, KnownFiles.DefaultDocument);
                isDefaultFile = true;
            }

            // try to determine if a reader accepts this request
            Readers.IContentReader? diskFileReader = null;
            IStorageItem? diskFile = null;
            foreach (var reader in _readers)
            {
                if (reader.AcceptRequest(file, out diskFile))
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
                    if (isDefaultFile)
                    {
                        var originalFile = _fileSystem.GetItem(path);
                        // should not happen, we juste made the same call at the beginning of this method
                        if (originalFile == null) throw new InvalidOperationException($"Unable to get originalFile for {path} despite having resolved it just now");
                        path = _fileSystem.Combine(originalFile, KnownFiles.DefaultMarkdown).RelativePath;
                    }


                    return new PageRender(PageRenderResult.Redirect) { RedirectUrl = $"{_settings.BaseUrl}/_manage/edit/{path}" };
                }

                // if the file does not exists, 404
                if (diskFile == null)
                {
                    return new PageRender(PageRenderResult.NotFound);
                }

                // if the file is a static resource, eg. not a markdown or html file we just return the content
                // we try to detect the content-type based on the extension
                if (!_contentTypeProvider.TryGetContentType(file.Extension, out var staticContentType))
                {
                    staticContentType = "application/octet-stream";
                }

                if (!_fileSystem.FileExists(diskFile))
                {
                    return new PageRender(PageRenderResult.NotFound);
                }

                return new PageRender(PageRenderResult.PhysicalFile) { PhysicalPath = diskFile.FullPath, ContentType = staticContentType };
            }

            // ok, now we start using the reader
            var readResult = await diskFileReader.ReadAsync(diskFile);

            if (readResult is Readers.RedirectReadResult actionResult)
            {
                return new PageRender(PageRenderResult.Redirect) { RedirectUrl = actionResult.Url };
            }

            var contentResult = (Readers.ContentReadResult)readResult;

            // git: retrieve latest commit
            var commit = _git.LogFile(diskFile.RelativePath, 1).FirstOrDefault();

            if (contentResult.ContentType == "text/html")
            {
                var render = new PageRender(PageRenderResult.Page)
                {
                    Html = contentResult.Content,
                    Items = contentResult.Headers,
                    Path = diskFile.RelativePath,
                    ManagePath = _fileSystem.GetDirectoryItem(diskFile).RelativePath,
                    LastUpdate = commit?.Authored ?? DateTimeOffset.MinValue,
                };

                // we automatically use the "title" yaml header as the Page Title
                if (contentResult.Headers.TryGetValue(KnownFields.Title, out var pageTitle) && pageTitle != null)
                {
                    render.Title = pageTitle.ToString() ?? string.Empty;
                }

                return render;
            }

            var contentType = contentResult.ContentType;

            // explicit content type found in headers
            if (contentResult.Headers.TryGetValue(KnownFields.ContentType, out var contentTypeObj) && contentTypeObj is string headerContentType)
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

            return new PageRender(PageRenderResult.Content) { Content = contentResult.Content, ContentType = contentType };
        }
    }
}
