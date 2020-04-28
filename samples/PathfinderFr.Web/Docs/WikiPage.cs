using System;
using System.Diagnostics;
using FlipLeaf.Storage;

namespace PathfinderFr.Docs
{
    [DebuggerDisplay("{Name} - {Title}")]
    public class WikiPage
    {
        private readonly IStorageItem _file;

        public WikiPage(IStorageItem file, string fullName, string title, DateTimeOffset lastModified, string[] categories)
        {
            _file = file;
            Name = new WikiName(fullName);
            Title = title;
            LastModified = lastModified;
            Categories = categories;
        }

        public WikiName Name { get; }

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

    [DebuggerDisplay("{FullName}")]
    public class WikiName
    {
        public WikiName(string name)
        {
            var i = name.IndexOf('#');
            if (i != -1)
            {
                if (i == name.Length - 1)
                {
                    name = name.TrimEnd('#');
                    Fragment = "#";
                }
                else if (i == 0)
                {
                    Fragment = name;
                    name = string.Empty;
                }
                else
                {
                    Fragment = name.Substring(i);
                    name = name.Substring(0, i);
                }
            }

            FullName = name ?? throw new ArgumentNullException(nameof(name));


            i = name.IndexOf('.');
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

        public WikiName(string @namespace, string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrEmpty(@namespace))
            {
                Namespace = string.Empty;
                Name = name;
                FullName = name;
            }
            else
            {
                Namespace = @namespace;
                Name = name;
                FullName = $"{Namespace}.{Name}";
            }
        }

        public string FullName { get; }

        public string Namespace { get; }

        public string Name { get; }

        public string Fragment { get; }

        public override string ToString() => FullName;


        public override int GetHashCode() => FullName.GetHashCode(StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj)
        {
            if (obj is WikiName name)
            {
                return string.Equals(FullName, name.FullName, StringComparison.OrdinalIgnoreCase);
            }

            return base.Equals(obj);
        }

        public static implicit operator string(WikiName name) => name.FullName;
        public static explicit operator WikiName(string name) => new WikiName(name);
    }
}
