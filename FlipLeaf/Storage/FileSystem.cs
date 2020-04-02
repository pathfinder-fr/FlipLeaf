using System;
using System.IO;
using System.Text;

namespace FlipLeaf.Storage
{
    public interface IFileSystem
    {
        IItemPath ItemPathFromFullPath(string fullPath);

        IItemPath ItemPathFromRelative(string relativePath);

        bool CheckFileExists(string path);

        string? GetDirectoryName(string? fullPath);

        string GetFullPath(string relativePath);

        string GetRelativePath(string fullPath);

        bool IsPathRooted(string fullPath);

        string GetExtension(string path);

        string ReadAllText(string fullPath, Encoding? encoding = null);

        string ReplaceExtension(string path, string newExtension, string? currentExt = null);
    }

    public class FileSystem : IFileSystem
    {
        private readonly string _basePath;

        public FileSystem(FlipLeafSettings settings)
        {
            _basePath = settings.SourcePath;
        }

        public string? GetDirectoryName(string? fullPath)
        {
            return Path.GetDirectoryName(fullPath);
        }

        public string GetFullPath(string relativePath)
        {
            return Path.Combine(_basePath, relativePath);
        }

        public bool IsPathRooted(string fullPath)
        {
            return new Uri(fullPath).LocalPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase);
        }

        public string GetExtension(string path)
        {
            return Path.GetExtension(path).ToLowerInvariant();
        }

        public string GetRelativePath(string fullPath)
        {
            if (!fullPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException();
            }

            var l = _basePath.Length;
            if (fullPath[l] == '/' || fullPath[l] == '\\')
                l++;

            return fullPath.Substring(l);
        }

        public string ReadAllText(string fullPath, Encoding? encoding = null)
        {
            return File.ReadAllText(fullPath, encoding ?? Encoding.UTF8);
        }

        public bool CheckFileExists(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                path = GetFullPath(path);
            }

            return File.Exists(path);
        }

        public string ReplaceExtension(string path, string newExtension, string? currentExt = null)
        {
            currentExt ??= GetExtension(path);

            return path[0..^currentExt.Length] + currentExt;
        }

        public IItemPath ItemPathFromFullPath(string fullPath)
        {
            return ItemPathFromRelative(GetRelativePath(fullPath));
        }

        public IItemPath ItemPathFromRelative(string relativePath)
        {
            return new ItemPath(
                Path.Combine(_basePath, relativePath),
                relativePath,
                Path.GetFileName(relativePath)
            );
        }

        public class ItemPath : IItemPath
        {
            public ItemPath(string fullPath, string relativePath, string name)
            {
                FullPath = fullPath;
                RelativePath = relativePath;
                Name = name;
            }

            public string Name { get; }

            public string RelativePath { get; }

            public string FullPath { get; }
        }
    }
}
