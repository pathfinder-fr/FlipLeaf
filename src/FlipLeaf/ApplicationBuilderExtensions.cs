using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace FlipLeaf
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseFlipLeaf(this IApplicationBuilder app, IWebHostEnvironment environment)
        {
            var settings = (FlipLeafSettings)app.ApplicationServices.GetService(typeof(FlipLeafSettings));

            // SourcePath validation
            var sourcePath = settings.SourcePath;
            if (!Path.IsPathRooted(sourcePath))
            {
                sourcePath = new Uri(Path.Combine(environment.ContentRootPath, sourcePath)).LocalPath;
            }

            if (!Directory.Exists(sourcePath))
            {
                Directory.CreateDirectory(sourcePath);
            }

            settings.SourcePath = sourcePath;

            // initialize website
            var fileSystem = app.ApplicationServices.GetService<Storage.IFileSystem>();
            var docStore = new Website.DocumentStore();
            foreach (var component in app.ApplicationServices.GetServices<Website.IWebsiteComponent>())
            {
                component.OnLoad(fileSystem, docStore);
            }
        }
    }
}
