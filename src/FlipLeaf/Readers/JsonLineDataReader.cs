using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using FlipLeaf.Storage;

namespace FlipLeaf.Readers
{
    /// <summary>
    /// A reader capable of handling json line files (.jsonl), as documented on http://jsonlines.org/.
    /// </summary>
    public class JsonLineDataReader : IDataReader
    {
        private readonly IFileSystem _fileSystem;

        public JsonLineDataReader(IFileSystem fileSystem)
        {
            this._fileSystem = fileSystem;
        }

        public bool Accept(IStorageItem file) => file.Extension.EqualsOrdinal(".jsonl", true);

        public void Read(IStorageItem file, IDictionary<string, object> data)
        {
            // we build a list with all values from the file
            var lines = new List<object>();
            using (var fs = _fileSystem.OpenRead(file))
            using (var reader = new StreamReader(fs))
            {
                string? jsonLine;
                while ((jsonLine = reader.ReadLine()) != null)
                {
                    var jsonDoc = JsonDocument.Parse(fs);
                    var doc = JsonDataReader.Convert(jsonDoc.RootElement);
                    if (doc != null)
                    {
                        lines.Add(doc);
                    }
                }
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
                // we handle the case where the value is already a list
                // in this case we append the values instead of overwriting them
                if (data.TryGetValue(key, out var existingValue) && existingValue is ICollection<object> existingList)
                {
                    foreach (var item in lines)
                    {
                        existingList.Add(item);
                    }
                }
                else
                {
                    // no value or not a list: we overwrite it
                    data[key] = lines;
                }
            }
        }
    }
}
