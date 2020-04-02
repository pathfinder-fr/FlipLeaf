using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FlipLeaf.Storage
{
    public interface IFileSystem
    {
        IStorageItem? GetItem(string relativePath);

        IStorageItem? GetFileFromSameDirectoryAs(IStorageItem file, string fileName, bool checkExists);

        IStorageItem Combine(IStorageItem item, string path);

        bool FileExists(IStorageItem file);

        bool DirectoryExists(IStorageItem directory);

        IEnumerable<IStorageItem> GetSubDirectories(IStorageItem directory, bool prefixDotIncluded, bool prefixUnderscoreIncluded);

        IEnumerable<IStorageItem> GetFiles(IStorageItem directory, bool prefixDotIncluded, bool prefixUnderscoreIncluded, string? pattern = null);

        void WriteAllText(IStorageItem file, string text, Encoding? encoding = null);

        IStorageItem ReplaceExtension(IStorageItem item, string newExtension);

        string ReadAllText(IStorageItem file, Encoding? encoding = null);
    }

    public class FileSystem : IFileSystem
    {
        private readonly char[] _invalidFileNameChars;
        private readonly char[] _invalidPathChars;
        private readonly string _basePath;
        private readonly IStorageItem _baseDir;

        public FileSystem(FlipLeafSettings settings)
        {
            _invalidFileNameChars = Path.GetInvalidFileNameChars();
            _invalidPathChars = Path.GetInvalidPathChars();

            _basePath = settings.SourcePath;
            _baseDir = new StorageItem(_basePath, string.Empty);
        }

        public IStorageItem? GetItem(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return _baseDir;
            }

            relativePath ??= string.Empty;
            var fullPath = Path.Combine(_basePath, relativePath);
            if (!CheckPathRooted(fullPath))
            {
                return null;
            }

            return new StorageItem(fullPath, relativePath);
        }

        public bool FileExists(IStorageItem file) => File.Exists(file.FullPath);

        public void WriteAllText(IStorageItem file, string text, Encoding? encoding = null)
        {
            EnsureDirectoryForFile(file.FullPath);
            File.WriteAllText(file.FullPath, text, encoding ?? Encoding.UTF8);
        }

        public string GetExtension(string path)
        {
            return Path.GetExtension(path).ToLowerInvariant();
        }

        public string ReadAllText(string fullPath, Encoding? encoding = null)
        {
            return File.ReadAllText(fullPath, encoding ?? Encoding.UTF8);
        }

        public string ReadAllText(IStorageItem file, Encoding? encoding = null)
        {
            return File.ReadAllText(file.FullPath, encoding ?? Encoding.UTF8);
        }

        public bool CheckFileExists(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                path = Path.GetFullPath(path);
            }

            return File.Exists(path);
        }

        public void WriteAllText(string fullPath, string text, Encoding? encoding = null)
        {
            EnsureDirectoryForFile(fullPath);
            File.WriteAllText(fullPath, text, encoding);
        }

        private void EnsureDirectoryForFile(string fullPath)
        {
            var dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        public IStorageItem? GetFileFromSameDirectoryAs(IStorageItem file, string fileName, bool checkExists)
        {
            CheckFileName(fileName);

            if (!File.Exists(file.FullPath))
            {
                return null;
            }

            var dir = Path.GetDirectoryName(file.FullPath);
            if (dir == null)
            {
                return null;
            }

            var targetFile = Path.Combine(dir, fileName);
            if (checkExists && !File.Exists(targetFile))
            {
                return null;
            }

            return GetItemFromFullPathUnsafe(targetFile);
        }

        public bool DirectoryExists(IStorageItem directory)
        {
            return Directory.Exists(directory.FullPath);
        }

        public IEnumerable<IStorageItem> GetSubDirectories(IStorageItem directory, bool prefixDotIncluded, bool prefixUnderscoreIncluded)
        {
            IEnumerable<IStorageItem> files = Directory
                .GetDirectories(directory.FullPath)
                .Where(d => d != null)
                .Select(d => GetItemFromFullPathUnsafe(d));

            if (!prefixDotIncluded)
            {
                files = files.Where(f => f.Name[0] != '.');
            }

            if (!prefixUnderscoreIncluded)
            {
                files = files.Where(f => f.Name[0] != '_');
            }

            return files;
        }

        public IEnumerable<IStorageItem> GetFiles(IStorageItem directory, bool prefixDotIncluded, bool prefixUnderscoreIncluded, string? pattern = null)
        {
            IEnumerable<IStorageItem> dirs = Directory
                .GetFiles(directory.FullPath, pattern ?? "*.*")
                .Where(f => f != null)
                .Select(f => GetItemFromFullPathUnsafe(f));

            if (!prefixDotIncluded)
            {
                dirs = dirs.Where(f => f.Name[0] != '.');
            }

            if (!prefixUnderscoreIncluded)
            {
                dirs = dirs.Where(f => f.Name[0] != '_');
            }

            return dirs;
        }

        public IStorageItem Combine(IStorageItem item, string path)
        {
            return GetItem(Path.Combine(item.RelativePath, path)) ?? throw new ArgumentException(nameof(path));
        }

        public IStorageItem ReplaceExtension(IStorageItem item, string newExtension)
        {
            var l = item.Extension.Length;
            if (string.IsNullOrEmpty(newExtension) || newExtension.Length < 2 || newExtension[0] != '.')
            {
                throw new ArgumentOutOfRangeException(nameof(newExtension), newExtension, $"Extension name {newExtension} is not valid. It must start with a dot and be at least one character long");
            }

            if (newExtension.IndexOfAny(_invalidFileNameChars) != -1)
            {
                throw new ArgumentOutOfRangeException(nameof(newExtension), newExtension, $"Extension name {newExtension} is not valid");
            }

            var newFullPath = item.FullPath[0..^l] + newExtension;

            return GetItemFromFullPathUnsafe(newFullPath);
        }

        private IStorageItem GetItemFromFullPathUnsafe(string fullPath)
        {
            if (!fullPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Invalid full path {fullPath}", nameof(fullPath));
            }

            var l = _basePath.Length;
            if (fullPath[l] == '/' || fullPath[l] == '\\')
                l++;

            var relativePath = fullPath.Substring(l);

            return new StorageItem(fullPath, relativePath);
        }

        private bool CheckPathRooted(string fullPath)
        {
            return new Uri(fullPath).LocalPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase);
        }

        private void CheckFileName(string fileName)
        {
            if (fileName.IndexOfAny(_invalidFileNameChars) != -1)
            {
                throw new ArgumentException($"Invalid file name : {fileName}", nameof(fileName));
            }
        }

        private class StorageItem : IStorageItem
        {
            private Lazy<string> _getName;
            private Lazy<string> _getExtension;

            public StorageItem(string fullPath, string relativePath)
            {
                FullPath = fullPath;
                RelativePath = relativePath.Replace('\\', '/');
                _getName = new Lazy<string>(GetName);
                _getExtension = new Lazy<string>(GetExtension);
            }

            public string FullPath { get; }

            public string RelativePath { get; }

            public string Name => _getName.Value;

            public string Extension => _getExtension.Value;

            private string GetName() => Path.GetFileName(FullPath);

            private string GetExtension() => Path.GetExtension(FullPath).ToLowerInvariant();
        }
    }
}
