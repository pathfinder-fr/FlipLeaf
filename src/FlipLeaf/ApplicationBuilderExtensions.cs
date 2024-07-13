using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FlipLeaf
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseFlipLeaf(this IApplicationBuilder app, IHostEnvironment environment)
        {
            var settings = app.ApplicationServices.GetRequiredService<FlipLeafSettings>();

            // validate and normalize source path
            settings.SourcePath = ValidateSourcePath(environment, settings.SourcePath);

            // initialize website
            ExecuteComponentLoad(app);
        }

        private static string ValidateSourcePath(IHostEnvironment environment, string sourcePath)
        {
            // make sourcePath absolute relative to ContentRootPath
            if (!Path.IsPathRooted(sourcePath))
                sourcePath = new Uri(Path.Combine(environment.ContentRootPath, sourcePath)).LocalPath;

            // ensure sourcePath exists
            if (!Directory.Exists(sourcePath))
                Directory.CreateDirectory(sourcePath);

            return sourcePath;
        }

        private static void ExecuteComponentLoad(IApplicationBuilder app)
        {
            var fileSystem = app.ApplicationServices.GetRequiredService<Storage.IFileSystem>();
            var website = app.ApplicationServices.GetRequiredService<Website.IWebsite>();

            foreach (var component in app.ApplicationServices.GetServices<Website.IWebsiteComponent>())
            {
                component.OnLoad(fileSystem, website);
            }
        }
    }
}
