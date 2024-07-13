using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace FlipLeaf
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseFlipLeaf(this IApplicationBuilder app, IWebHostEnvironment environment)
        {
            // SourcePath validation
            var settings = (FlipLeafSettings?)app.ApplicationServices.GetService(typeof(FlipLeafSettings));
            if (settings == null)
            {
                throw new InvalidOperationException($"Type FlipLeafSettings must be registered");
            }

            settings.SourcePath = ValidateSourcePath(environment, settings.SourcePath);

            // initialize website
            ExecuteComponentLoad(app);
        }

        private static string ValidateSourcePath(IWebHostEnvironment environment, string sourcePath)
        {
            if (!Path.IsPathRooted(sourcePath))
            {
                sourcePath = new Uri(Path.Combine(environment.ContentRootPath, sourcePath)).LocalPath;
            }

            if (!Directory.Exists(sourcePath))
            {
                Directory.CreateDirectory(sourcePath);
            }

            return sourcePath;
        }

        private static void ExecuteComponentLoad(IApplicationBuilder app)
        {
            var fileSystem = app.ApplicationServices.GetService<Storage.IFileSystem>();
            var website = app.ApplicationServices.GetService<Website.IWebsite>();

            if (fileSystem == null || website == null) return;

            var components = app.ApplicationServices.GetServices<Website.IWebsiteComponent>();

            foreach (var component in components)
            {
                component.OnLoad(fileSystem, website);
            }
        }
    }
}
