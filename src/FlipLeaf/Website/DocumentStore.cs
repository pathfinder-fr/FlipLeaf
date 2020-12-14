using System;
using System.Collections.Generic;
using FlipLeaf.Docs;

namespace FlipLeaf.Website
{
    public sealed class DocumentStore<T>
            where T : class, IDocument
    {
        private readonly Dictionary<string, T> _store = new(StringComparer.Ordinal);

        public IEnumerable<T> GetAll() => _store.Values;

        public T? Get(string name) => _store[name];

        public void Add(T document)
        {
            lock (this)
            {
                _store.Add(document.Name, document);
            }
        }
    }
}
