namespace FlipLeaf.Storage
{
    public interface IItemPath
    {
        /// <summary>
        /// Name of file or directory.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Relative path of the file from the base directory.
        /// </summary>
        string RelativePath { get; }

        /// <summary>
        /// Full path of the file on disk.
        /// </summary>
        string FullPath { get; }
    }
}
