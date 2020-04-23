using System.Collections.Generic;

namespace FlipLeaf.Storage
{
    /// <summary>
    /// Represents an item in the storage disk.
    /// Can be a file or a directory.
    /// An instance does not suppose that the item really exists on disk, only that it represents a valid item in term of naming rules
    /// and is stored inside the base directory for the site.
    /// </summary>
    public interface IStorageItem
    {
        FamilyFolder FamilyFolder { get; }

        /// <summary>
        /// Name of file or directory.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Extension of the file.
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// Relative path of the file or directory from the base directory.
        /// Always use url path separator '/'.
        /// Does not start with '/'.
        /// </summary>
        string RelativePath { get; }

        /// <summary>
        /// Full path of the file on disk.
        /// Always use windows directory separator '\'.
        /// </summary>
        string FullPath { get; }
    }

    public static class StorageItemExtensions
    {
        public static bool IsMarkdown(this IStorageItem @this) => @this.Extension == ".md";

        public static bool IsHtml(this IStorageItem @this) => @this.Extension == ".html";

        public static bool IsYaml(this IStorageItem @this) => @this.Extension == ".yml" || @this.Extension == ".yaml";

        public static bool IsJson(this IStorageItem @this) => @this.Extension == ".json";

        public static bool IsXml(this IStorageItem @this) => @this.Extension == ".xml";

        public static IEnumerable<string> RelativeDirectoryParts(this IStorageItem @this)
        {
            var relPath = @this.RelativePath;
            int i = 0;
            int j;
            while ((j = relPath.IndexOf('/', i)) != -1)
            {
                yield return relPath[i..j];
                i = j + 1;
            }
        }
    }
}
