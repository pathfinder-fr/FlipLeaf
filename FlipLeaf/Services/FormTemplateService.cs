using System.IO;
using FlipLeaf.Services.FormTemplating;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlipLeaf.Services
{
    public interface IFormTemplateService
    {
        FormTemplate ParseTemplate(string path);
    }

    public class FormTemplateService : IFormTemplateService
    {
        private readonly string _basePath;

        public FormTemplateService(FlipLeafSettings settings)
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