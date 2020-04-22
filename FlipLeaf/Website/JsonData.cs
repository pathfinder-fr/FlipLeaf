using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace FlipLeaf.Website
{
    public class JsonData
    {
        private readonly IDictionary<string, object> _data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        private readonly string _path;

        public JsonData(string path)
        {
            _path = path;
        }

        public void Parse(string sourcePath)
        {
            JsonDocument data;
            using (var fs = File.OpenRead(_path))
            {
                data = JsonDocument.Parse(fs);
            }

            var container = (IDictionary<string, object?>)_data;

            var directory = Path.GetDirectoryName(_path);
            var dataDir = Path.Combine(sourcePath, KnownFolders.Data);
            if (directory != null && directory.Length > dataDir.Length)
            {
                var relativePath = directory.Substring(dataDir.Length + 1);
                var parts = relativePath.Split(new char[] { '/', '\\' });
                if (parts.Length > 0)
                {
                    for (int i = 0; i < parts.Length; i++)
                    {
                        var partKey = parts[i];
                        if (!_data.TryGetValue(partKey, out var partValue) || !(partValue is IDictionary<string, object?> partDict))
                        {
                            partDict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                            container[partKey] = partDict;
                        }

                        container = partDict;
                    }
                }
            }


            var key = Path.GetFileNameWithoutExtension(_path);

            object? value;
            if (!container.TryGetValue(key, out var dataItem) || dataItem == null)
            {
                value = Convert(data.RootElement);
            }
            else
            {
                value = Merge(dataItem, data.RootElement);
            }

            if (value != null)
            {
                container[key] = value;
            }
        }

        private object? Merge(object source, JsonElement jsonElement)
        {
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Object:
                    if (source is IDictionary<string, object?> sourceDict)
                    {
                        foreach (var jsonProperty in jsonElement.EnumerateObject())
                        {
                            if (sourceDict.TryGetValue(jsonProperty.Name, out var sourceDictValue) && sourceDictValue != null)
                            {
                                sourceDict[jsonProperty.Name] = Merge(sourceDictValue, jsonProperty.Value);
                            }
                            else
                            {
                                sourceDict[jsonProperty.Name] = Convert(jsonProperty.Value);
                            }
                        }

                        return sourceDict;
                    }
                    else
                    {
                        // overwrite
                        return Convert(jsonElement);
                    }

                default:
                    // overwrite
                    return Convert(jsonElement);
            }
        }

        private object? Convert(JsonElement jsonElement)
        {
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Array:
                    var listResult = new List<object?>(jsonElement.GetArrayLength());

                    foreach (var arrayItem in jsonElement.EnumerateArray())
                    {
                        listResult.Add(Convert(arrayItem));
                    }

                    return listResult;

                case JsonValueKind.False: return false;
                case JsonValueKind.Null: return null;
                case JsonValueKind.Number: return jsonElement.GetInt32();
                case JsonValueKind.String: return jsonElement.GetString();
                case JsonValueKind.True: return true;
                case JsonValueKind.Object:
                    var dictResult = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                    foreach (var property in jsonElement.EnumerateObject())
                    {
                        dictResult[property.Name] = Convert(property.Value);
                    }

                    return dictResult;
            }

            return null;
        }

    }
}
