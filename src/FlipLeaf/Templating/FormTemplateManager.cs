using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using FlipLeaf.Storage;
using FlipLeaf.Website;
using YamlDotNet.Serialization;

namespace FlipLeaf.Templating
{
    public class FormTemplateManager : IWebsiteComponent
    {
        private readonly Dictionary<string, FormTemplate> _templates = new Dictionary<string, FormTemplate>(StringComparer.OrdinalIgnoreCase);

        public void OnLoad(IFileSystem fileSystem, IDocumentStore docs)
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
                        var doc = new Docs.Template(file, template);
                        docs.Add(doc);
                    }
                }
            }
        }

        private FormTemplate ParseJsonTemplate(IFileSystem fileSystem, IStorageItem item)
        {
            var templateSource = fileSystem.ReadAllText(item);

            var formTemplate = JsonSerializer.Deserialize<FormTemplate>(templateSource, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            });

            if (formTemplate == null)
            {
                throw new ArgumentException($"Unable to load template from json item {item}");
            }

            return formTemplate;
        }

        private FormTemplate ParseYamlTemplate(IFileSystem fileSystem, IStorageItem item)
        {
            var templateSource = fileSystem.ReadAllText(item);
            var deserializer = new Deserializer();
            return deserializer.Deserialize<FormTemplate>(templateSource);
        }
    }
}
