using System;
using System.Collections.Generic;
using System.Linq;

namespace FlipLeaf
{
    public sealed class HeaderFieldDictionary : Dictionary<string, object?>
    {
        public HeaderFieldDictionary()
            : base(StringComparer.Ordinal)
        {
        }

        public T[] GetArray<T>(string name) => GetCollection<T>(name).ToArray();

        public IEnumerable<T> GetCollection<T>(string name)
        {
            var objects = this.GetValueOrDefault(name) as IEnumerable<object>;
            if (objects == null)
                return Enumerable.Empty<T>();

            return objects.Cast<T>();
        }
    }
}
