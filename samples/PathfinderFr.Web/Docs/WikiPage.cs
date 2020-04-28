using System;
using FlipLeaf.Storage;

namespace PathfinderFr.Docs
{
    public class WikiPage
    {
        private readonly IStorageItem _file;

        public WikiPage(IStorageItem file, string fullName, string title, DateTimeOffset lastModified, string[] categories)
        {
            _file = file;
            FullName = fullName;
            Title = title;
            LastModified = lastModified;
            Categories = categories;
        }

        public string FullName { get; }

        public string Title { get; }

        public DateTimeOffset LastModified { get; }

        public string[] Categories { get; }
    }

    public class WikiSnippet
    {
        public WikiSnippet(string content)
        {
            Content = content;
        }

        public string Content { get; }
    }

    public class WikiName
    {
        public WikiName(string name)
        {
            FullName = name ?? throw new ArgumentNullException(nameof(name));
            var i = name.IndexOf('.');
            if (i == -1)
            {
                Namespace = string.Empty;
                Name = name;
            }
            else if (i == 0)
            {
                Namespace = string.Empty;
                Name = name.Substring(1);
            }
            else if (i == name.Length - 1)
            {
                Namespace = name[0..^1];
                Name = string.Empty;
            }
            else
            {
                Namespace = name.Substring(0, i);
                Name = name.Substring(i + 1);
            }
        }

        public string FullName { get; }
        public string Namespace { get; }
        public string Name { get; }
        public override string ToString() => FullName;

        public static implicit operator string(WikiName name) => name.FullName;
        public static explicit operator WikiName(string name) => new WikiName(name);
    }
}
