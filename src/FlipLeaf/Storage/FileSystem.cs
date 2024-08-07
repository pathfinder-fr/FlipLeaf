﻿using System.Text;

namespace FlipLeaf.Storage
{
    public interface IFileSystem
    {
        IStorageItem? GetItem(string relativePath);

        IStorageItem? GetLayout(string name);

        IStorageItem? GetTemplate(string name);

        IStorageItem? GetFileFromSameDirectoryAs(IStorageItem file, string fileName, bool checkExists);

        IStorageItem Combine(IStorageItem item, string path);

        bool FileExists(IStorageItem file);

        bool DirectoryExists(IStorageItem directory);

        IEnumerable<IStorageItem> GetSubDirectories(IStorageItem directory, bool prefixDotIncluded = false, bool prefixUnderscoreIncluded = false);

        IEnumerable<IStorageItem> GetFiles(IStorageItem directory, bool prefixDotIncluded = false, bool prefixUnderscoreIncluded = false, string? pattern = null);

        void WriteAllText(IStorageItem file, string text, Encoding? encoding = null);

        IStorageItem ReplaceExtension(IStorageItem item, string newExtension);

        string ReadAllText(IStorageItem file, Encoding? encoding = null);

        Task<string> ReadAllTextAsync(IStorageItem file, Encoding? encoding = null);

        TextReader OpenTextReader(IStorageItem file, Encoding? encoding = null);

        Stream OpenRead(IStorageItem file);

        /// <summary>
        /// Returns the directory containing the file <paramref name="file"/>.
        /// </summary>
        IStorageItem GetDirectoryItem(IStorageItem file);

        void EnsureDirectory(IStorageItem directory);

        bool DeleteFile(IStorageItem file);
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

            while (relativePath.Length > 1 && relativePath[0] == '/')
            {
                relativePath = relativePath.Substring(1);
            }

            if (relativePath.Length > 0 && relativePath[0] == '/')
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

        public IStorageItem? GetLayout(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"Name can not be empty", nameof(name));

            if (name.IndexOfAny(_invalidFileNameChars) != -1)
                throw new ArgumentException($"{name} is not a valid layout name", nameof(name));

            if (Path.GetExtension(name) != ".")
                throw new ArgumentException($"The layout name must not include the file extension. Use {Path.GetFileNameWithoutExtension(name)} instead of {name}", nameof(name));

            return GetExistingItemUnsafe($"{KnownFolders.Layouts}/{name}.html");
        }

        public IStorageItem? GetTemplate(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"Name can not be empty", nameof(name));

            if (name.IndexOfAny(_invalidFileNameChars) != -1)
                throw new ArgumentException($"{name} is not a valid template name", nameof(name));

            if (!string.IsNullOrEmpty(Path.GetExtension(name)))
                throw new ArgumentException($"The template name must not include the file extension. Use {Path.GetFileNameWithoutExtension(name)} instead of {name}", nameof(name));

            IStorageItem? item;

            // json first
            item = GetExistingItemUnsafe($"{KnownFolders.Templates}/{name}.json");
            if (item != null)
            {
                return item;
            }

            // yaml then
            item = GetExistingItemUnsafe($"{KnownFolders.Templates}/{name}.yaml");
            if (item != null)
            {
                return item;
            }

            // yaml then
            item = GetExistingItemUnsafe($"{KnownFolders.Templates}/{name}.yml");
            if (item != null)
            {
                return item;
            }

            return null;
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

        public Task<string> ReadAllTextAsync(IStorageItem file, Encoding? encoding = null)
        {
            return File.ReadAllTextAsync(file.FullPath, encoding ?? Encoding.UTF8);
        }

        public TextReader OpenTextReader(IStorageItem file, Encoding? encoding = null)
        {
            return new StreamReader(file.FullPath, encoding ?? Encoding.UTF8);
        }

        public Stream OpenRead(IStorageItem file)
        {
            return new FileStream(file.FullPath, FileMode.Open, FileAccess.Read);
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
            File.WriteAllText(fullPath, text, encoding ?? Encoding.UTF8);
        }

        private void EnsureDirectoryForFile(string fullPath)
        {
            var dir = Path.GetDirectoryName(fullPath);
            if (dir == null)
            {
                throw new ArgumentException($"Unable to obtain directory for path {fullPath}");
            }

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
            if (!Directory.Exists(directory.FullPath))
            {
                return Enumerable.Empty<IStorageItem>();
            }

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

        private IStorageItem? GetExistingItemUnsafe(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return _baseDir;
            }

            var fullPath = Path.Combine(_basePath, relativePath);
            if (File.Exists(fullPath))
            {
                return new StorageItem(fullPath, relativePath);
            }

            return null;
        }

        private IStorageItem GetItemFromFullPathUnsafe(string fullPath)
        {
            if (!fullPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Invalid full path {fullPath}", nameof(fullPath));
            }

            var l = _basePath.Length;
            if (fullPath.Length > l)
            {
                if (fullPath[l] == '/' || fullPath[l] == '\\')
                {
                    l++;
                }
            }

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

        public IStorageItem GetDirectoryItem(IStorageItem file)
        {
            var directoryPath = Path.GetDirectoryName(file.FullPath);
            if (directoryPath == null)
            {
                throw new InvalidOperationException($"Unable to get directory for file {file}");
            }

            return GetItemFromFullPathUnsafe(directoryPath);
        }

        public void EnsureDirectory(IStorageItem directory)
        {
            if (directory == null)
            {
                throw new ArgumentOutOfRangeException(nameof(directory), directory, $"Invalid directory name");
            }

            if (FileExists(directory))
            {
                throw new ArgumentOutOfRangeException(nameof(directory), directory, $"Unable to create sub directory : a file with the same name alread exists");
            }

            if (!DirectoryExists(directory))
            {
                Directory.CreateDirectory(directory.FullPath);
            }
        }

        public bool DeleteFile(IStorageItem file)
        {
            if (!FileExists(file))
            {
                return false;
            }

            File.Delete(file.FullPath);
            return true;
        }

        private class StorageItem : IStorageItem
        {
            private Lazy<string> _getName;
            private Lazy<string> _getExtension;
            private Lazy<FamilyFolder> _getFamilyFolder;

            public StorageItem(string fullPath, string relativePath)
            {
                if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));
                if (relativePath == null) throw new ArgumentNullException(nameof(relativePath));

                FullPath = fullPath.Replace('/', '\\');
                RelativePath = relativePath.Replace('\\', '/');

                _getFamilyFolder = new Lazy<FamilyFolder>(GetFamilyFolder);
                _getName = new Lazy<string>(GetName);
                _getExtension = new Lazy<string>(GetExtension);
            }

            public FamilyFolder FamilyFolder { get; }

            public string FullPath { get; }

            public string RelativePath { get; }

            public string Name => _getName.Value;

            public string Extension => _getExtension.Value;

            private string GetName() => Path.GetFileName(FullPath);

            private string GetExtension() => Path.GetExtension(FullPath).ToLowerInvariant();

            private FamilyFolder GetFamilyFolder()
            {
                var i = RelativePath.IndexOf('/');
                if (i == -1 || i == 0)
                {
                    return FamilyFolder.None;
                }

                return RelativePath.Substring(0, i) switch
                {
                    KnownFolders.Data => FamilyFolder.Data,
                    KnownFolders.Includes => FamilyFolder.Includes,
                    KnownFolders.Layouts => FamilyFolder.Layouts,
                    KnownFolders.Templates => FamilyFolder.Templates,
                    _ => FamilyFolder.None,
                };
            }

            public override string ToString() => RelativePath;

            public override int GetHashCode() => RelativePath.GetHashCode(StringComparison.Ordinal);

            public override bool Equals(object? obj)
            {
                if (obj is IStorageItem item)
                {
                    return item.RelativePath.EqualsOrdinal(RelativePath);
                }

                return base.Equals(obj);
            }
        }
    }
}
