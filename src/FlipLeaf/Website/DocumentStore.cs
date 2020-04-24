using System;
using System.Collections.Generic;
using System.Linq;
using FlipLeaf.Docs;

namespace FlipLeaf.Website
{
    public interface IDocumentStore
    {
        IEnumerable<T> GetAll<T>() where T : class, IDocument;

        void Add<T>(T document) where T : class, IDocument;
    }

    public sealed class DocumentStore : IDocumentStore
    {
        private Dictionary<Type, Dictionary<string, IDocument>> _store = new Dictionary<Type, Dictionary<string, IDocument>>();

        //private Dictionary<IDocument, HashSet<IDocument>> _dependsOn = new Dictionary<IDocument, HashSet<IDocument>>();
        //private Dictionary<IDocument, HashSet<IDocument>> _isDependencyOf = new Dictionary<IDocument, HashSet<IDocument>>();

        public IEnumerable<T> GetAll<T>()
            where T : class, IDocument
        {
            if (!_store.TryGetValue(typeof(T), out var docs))
            {
                return Enumerable.Empty<T>();
            }

            return docs.Values.Cast<T>();
        }

        public T? Get<T>(string name)
            where T : class, IDocument
        {
            if (!_store.TryGetValue(typeof(T), out var docs))
            {
                return default;
            }

            return (T)docs[name];
        }

        public void Add<T>(T document)
            where T : class, IDocument => Add(typeof(T), document);

        public void Add(Type type, IDocument doc)
        {
            lock (this)
            {
                if (!_store.TryGetValue(type, out var set))
                {
                    _store[type] = set = new Dictionary<string, IDocument>();
                }

                set.Add(doc.Name, doc);
            }
        }
    }
}
