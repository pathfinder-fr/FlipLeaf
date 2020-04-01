using System;
using System.IO;

namespace FlipLeaf.Models
{
    public class ItemPath
    {
        public ItemPath(string basePath, string path)
        {
            Name = Path.GetFileName(path);
            RelativePath = path.Replace('\\', '/');
            FullPath = Path.Combine(basePath, path);
        }

        /// <summary>
        /// Name of file or directory.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Relative path of the file from the base directory.
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        /// Full path of the file on disk.
        /// </summary>
        public string FullPath { get; }

        public static ItemPath FromFullPath(string basePath, string fullPath)
        {
            if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException();

            var l = basePath.Length;
            if (fullPath[l] == '/' || fullPath[l] == '\\')
                l++;

            return new ItemPath(basePath, fullPath.Substring(l));
        }
    }
}
