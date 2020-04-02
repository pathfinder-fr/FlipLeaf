using System.Text.Json;
using System.Text.Json.Serialization;
using FlipLeaf.Rendering.FormTemplating;
using FlipLeaf.Storage;

namespace FlipLeaf.Rendering
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

            var templateJson = _fileSystem.ReadAllText(item);

            return JsonSerializer.Deserialize<FormTemplate>(templateJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            });
        }
    }
}
