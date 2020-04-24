using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using FlipLeaf.Storage;

namespace FlipLeaf.Readers
{
    public class JsonDataReader : IDataReader
    {
        private readonly IFileSystem _fileSystem;

        public JsonDataReader(IFileSystem fileSystem)
        {
            this._fileSystem = fileSystem;
        }

        public bool Accept(IStorageItem file) => file.IsJson();

        public void Read(IStorageItem file, IDictionary<string, object> data)
        {
            JsonDocument jsonDoc;
            using (var fs = _fileSystem.OpenRead(file))
            {
                jsonDoc = JsonDocument.Parse(fs);
            }

            var i = 0;
            foreach (var part in file.RelativeDirectoryParts())
            {
                if (i != 0)
                {
                    if (!data.TryGetValue(part, out var partValue) || !(partValue is IDictionary<string, object> partDict))
                    {
                        partDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                        data[part] = partDict;
                    }

                    data = partDict;
                }

                i++;
            }

            var key = Path.GetFileNameWithoutExtension(file.Name);

            object? value;
            if (!data.TryGetValue(key, out var dataItem) || dataItem == null)
            {
                value = Convert(jsonDoc.RootElement);
            }
            else
            {
                value = Merge(dataItem, jsonDoc.RootElement);
            }

            if (value != null)
            {
                data[key] = value;
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
