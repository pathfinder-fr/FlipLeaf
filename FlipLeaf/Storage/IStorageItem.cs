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
        /// </summary>
        string RelativePath { get; }

        /// <summary>
        /// Full path of the file on disk.
        /// </summary>
        string FullPath { get; }
    }

    public static class StorageItemExtensions
    {
        public static bool IsMarkdown(this IStorageItem @this) => @this.Extension == ".md";

        public static bool IsHtml(this IStorageItem @this) => @this.Extension == ".html";
    }
}
