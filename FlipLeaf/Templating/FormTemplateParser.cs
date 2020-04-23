using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using FlipLeaf.Storage;
using YamlDotNet.Serialization;

namespace FlipLeaf.Templating
{
    public interface IFormTemplateParser
    {
        FormTemplate ParseTemplate(string path);
    }

    public class FormTemplateParser : IFormTemplateParser
    {
        private readonly IFileSystem _fileSystem;

        public FormTemplateParser(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public FormTemplate ParseTemplate(string path)
        {
            var item = _fileSystem.GetItem(path);
            if (item == null || !_fileSystem.FileExists(item))
            {
                return FormTemplate.Default;
            }

            var templateSource = _fileSystem.ReadAllText(item);

            if (item.IsJson())
            {
                return JsonSerializer.Deserialize<FormTemplate>(templateSource, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                });
            }
            else if(item.IsYaml())
            {
                var deserializer = new Deserializer();
                return deserializer.Deserialize<FormTemplate>(templateSource);
            }

            throw new NotSupportedException($"Template with extension {item.Extension} are not supported");
        }
    }
}
