using System;
using System.IO;

namespace FlipLeaf
{
    public partial class Engine
    {
        private const string DefaultLayoutsFolder = "_layouts";
        private const string DefaultOutputFolder = "_site";
        private readonly IRenderingMiddleware[] _middlewares = {
            new YamlHeaderRenderer(),
            new FluidRenderer(),
            new MarkdownRenderer()
            };

        private readonly string _root;

        private string _outputDir;

        public Engine(string root)
        {
            _root = root;
        }

        public SiteSettings Site { get; set; } = new SiteSettings();

        public void Init()
        {
            CompileConfig();
        }

        private void CompileConfig()
        {
            var path = Path.Combine(_root, "_config.yml");
            if (!File.Exists(path))
                return;

            this.Site = Yaml.ParseConfig(path);
        }

        public void RenderAll(string outputDir = DefaultOutputFolder)
        {
            _outputDir = outputDir;
            RenderFolder(string.Empty);
        }

        private void RenderFolder(string directory)
        {
            var dir = Path.Combine(_root, directory);

            var targetDir = Path.Combine(_root, _outputDir, directory);
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            foreach (var file in Directory.GetFiles(dir))
            {
                var fileName = Path.GetFileName(file);
                var fileExtension = Path.GetExtension(file);

                if (fileExtension == ".md")
                {
                    var targetPath = Path.Combine(targetDir, Path.ChangeExtension(fileName, ".html"));
                    RenderToFile(Path.Combine(directory, fileName), targetPath);
                }
                else
                {
                    File.Copy(file, Path.Combine(targetDir, fileName), true);
                }
            }

            foreach (var subDir in Directory.GetDirectories(dir))
            {
                if (!string.IsNullOrEmpty(directory))
                {
                    RenderFolder(Path.Combine(directory, subDir));
                }
                else
                {
                    var directoryName = Path.GetFileName(subDir);

                    if (string.Equals(directoryName, _outputDir, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (string.Equals(directoryName, DefaultLayoutsFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    RenderFolder(Path.Combine(directory, subDir));
                }
            }
        }

        public void RenderToFile(string pagePath, string targetPath)
        {
            // make absolute
            var path = Path.Combine(_root, pagePath);

            // render
            var content = Render(path);

            // write
            File.WriteAllText(targetPath, content);
        }

        /// <summary>
        /// Render the content of the page and returns the result.
        /// </summary>
        public string Render(string path)
        {
            var app = new AppEngine(_middlewares);

            using (var output = app.Execute(File.OpenRead(path)))
            {
                output.Position = 0;
                return new StreamReader(output).ReadToEnd();
            }
        }
    }
}