using System.IO;
using FlipLeaf.Rendering.FormTemplating;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlipLeaf.Rendering
{
    public interface IFormTemplateParser
    {
        FormTemplate ParseTemplate(string path);
    }

    public class FormTemplateParser : IFormTemplateParser
    {
        private readonly string _basePath;

        public FormTemplateParser(FlipLeafSettings settings)
        {
            _basePath = settings.SourcePath;
        }

        public FormTemplate ParseTemplate(string path)
        {
            var fullPath = Path.Combine(_basePath, path);
            if (!File.Exists(fullPath))
            {
                return FormTemplate.Default;
            }

            var templateJson = File.ReadAllText(fullPath);

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
