using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

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
        }
    }
}
