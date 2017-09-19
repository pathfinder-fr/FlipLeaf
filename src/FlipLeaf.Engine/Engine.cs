using System;
using System.IO;

namespace FlipLeaf
{
    public partial class Engine
    {
        private const string DefaultLayoutsFolder = "_layouts";
        private const string DefaultOutputFolder = "_site";

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
            var source = File.ReadAllText(path);

            // 1) yaml
            if (!Yaml.ParseHeader(ref source, out var pageContext))
            {
                return null;
            }

            // 2) fluid
            if (!Fluid.ParsePage(ref source, pageContext, Site, out var templateContext))
            {
                return null;
            }

            // 3) markdown
            if (!Markdown.Parse(ref source))
            {
                return null;
            }

            // 4) layout
            if (!Fluid.ApplyLayout(ref source, templateContext, _root))
            {
                return null;
            }

            return source;
        }
    }
}