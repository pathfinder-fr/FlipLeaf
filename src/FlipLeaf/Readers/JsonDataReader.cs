using System.Text.Json;
using FlipLeaf.Storage;

namespace FlipLeaf.Readers
{
    public class JsonDataReader : IDataReader
    {
        private readonly IFileSystem _fileSystem;

        public JsonDataReader(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public bool Accept(IStorageItem file) => file.IsJson();

        public void Read(IStorageItem file, IDictionary<string, object> data)
        {
            // read json document
            JsonDocument jsonDoc;
            using (var fs = _fileSystem.OpenRead(file))
            {
                jsonDoc = JsonDocument.Parse(fs);
            }

            // find valid data position by traversing path and filename
            // we ignore the first part (folder _data)
            var parts = file.RelativeDirectoryParts().Skip(1);

            // we add the filename as a part of the key unless the filename starts with character '_' : 
            // it will allow creating a directory with multiple files for the same array, if all files starts with _.
            if (!file.Name.StartsWith("_"))
            {
                parts = parts.Append(Path.GetFileNameWithoutExtension(file.Name));
            }

            // the key for list will be stored in the last part of the path
            var key = parts.Last();

            // we browse each part to resolve the dictionary where the list must be set
            foreach (var part in parts.SkipLast(1))
            {
                if (!data.TryGetValue(part, out var partValue) || !(partValue is IDictionary<string, object> partDict))
                {
                    partDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    data[part] = partDict;
                }

                data = partDict;
            }


            if (data != null)
            {
                object? value;
                if (data.TryGetValue(key, out var existingValue) && existingValue != null)
                {
                    value = Merge(existingValue, jsonDoc.RootElement);
                }
                else
                {
                    value = Convert(jsonDoc.RootElement);
                }

                if (value != null)
                {
                    data[key] = value;
                }
            }
        }

        internal static object? Merge(object source, JsonElement jsonElement)
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

        internal static object? Convert(JsonElement jsonElement)
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
