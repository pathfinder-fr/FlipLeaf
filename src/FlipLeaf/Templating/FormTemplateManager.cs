using System.Text.Json;
using System.Text.Json.Serialization;
using FlipLeaf.Markup;
using FlipLeaf.Storage;
using FlipLeaf.Website;
using YamlDotNet.Serialization;

namespace FlipLeaf.Templating
{
    public interface IFormTemplateManager
    {
        bool TryLoadTemplate(string? templateName, IStorageItem file, out FormTemplatePage? templatePage);
    }

    public class FormTemplateManager : IFormTemplateManager, IWebsiteComponent
    {
        private readonly Dictionary<string, FormTemplate> _templates = new(StringComparer.OrdinalIgnoreCase);
        private readonly IFileSystem _fileSystem;
        private readonly IYamlMarkup _yaml;

        public FormTemplateManager(IFileSystem _fileSystem, IYamlMarkup yaml)
        {
            this._fileSystem = _fileSystem;
            _yaml = yaml;
        }

        public bool TryLoadTemplate(string? templateName, IStorageItem file, out FormTemplatePage? templatePage)
        {
            HeaderFieldDictionary yamlHeader;
            string content;

            if (_fileSystem.FileExists(file) && templateName == null)
            {
                // read raw content
                content = _fileSystem.ReadAllText(file);

                // parse YAML header
                yamlHeader = _yaml.ParseHeader(content, out content);

                if (yamlHeader.TryGetValue(KnownFields.Template, out var templateNameObj))
                {
                    templateName = templateNameObj as string;
                }
            }
            else
            {
                content = string.Empty;
                yamlHeader = new HeaderFieldDictionary();
            }

            if (templateName == null)
            {
                templatePage = null;
                return false;
            }

            // try load template
            if (!_templates.TryGetValue(templateName, out var formTemplate))
            {
                templatePage = null;
                return false;
            }

            // parse template
            templatePage = new FormTemplatePage(templateName, formTemplate, yamlHeader, content);
            return true;
        }

        public void OnLoad(IFileSystem fileSystem, IWebsite website)
        {
            _templates.Clear();
            var dirItem = fileSystem.GetItem(KnownFolders.Templates);
            if (dirItem != null && fileSystem.DirectoryExists(dirItem))
            {
                foreach (var file in fileSystem.GetFiles(dirItem))
                {
                    FormTemplate? template = null;
                    if (file.IsJson())
                    {
                        template = ParseJsonTemplate(fileSystem, file);
                    }
                    else if (file.IsYaml())
                    {
                        template = ParseYamlTemplate(fileSystem, file);
                    }

                    if (template != null)
                    {
                        var name = System.IO.Path.GetFileNameWithoutExtension(file.Name).ToLowerInvariant();
                        _templates[name] = template;
                    }
                }
            }
        }

        private static FormTemplate ParseJsonTemplate(IFileSystem fileSystem, IStorageItem item)
        {
            var templateSource = fileSystem.ReadAllText(item);

            var formTemplate = JsonSerializer.Deserialize<FormTemplate>(templateSource, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },

            });

            if (formTemplate == null)
            {
                throw new ArgumentException($"Unable to load template from json item {item}");
            }

            return formTemplate;
        }

        private static FormTemplate ParseYamlTemplate(IFileSystem fileSystem, IStorageItem item)
        {
            var templateSource = fileSystem.ReadAllText(item);
            var deserializer = new Deserializer();
            return deserializer.Deserialize<FormTemplate>(templateSource);
        }
    }
}
