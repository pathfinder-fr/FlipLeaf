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
            if (!(this.GetValueOrDefault(name) is IEnumerable<object> objects))
            {
                return Enumerable.Empty<T>();
            }

            return objects.Cast<T>();
        }
    }
}
