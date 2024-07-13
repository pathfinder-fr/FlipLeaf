using System.IO;
using System.Linq;
using Xunit;

namespace FlipLeaf.Storage
{
    public class IStorageItemTests
    {
        [Theory]
        [InlineData(@"c:\test", @"c:\test", 0)]
        [InlineData(@"c:\test", @"c:\test\", 0)]
        [InlineData(@"c:\test", @"c:\test\\", 1)]
        [InlineData(@"c:\test", @"c:\test\file.ext", 0)]
        [InlineData(@"c:\test", @"c:\test\my\file.ext", 1)]
        [InlineData(@"c:\test", @"c:\test\my\\third\file.ext", 3)]
        [InlineData(@"c:\test", @"c:\test\my\\third\file.ext\", 4)]
        [InlineData(@"c:\test", @"c:\test\my\second\file.ext", 2)]
        [InlineData(@"c:\test", @"c:\test\my\second\third\file.ext", 3)]
        public void RelativeDirectoryParts_Length(string @base, string path, int partCount)
        {
            var item = new StorageItemStub(@base, path);

            var parts = item.RelativeDirectoryParts().ToArray();

            Assert.Equal(partCount, parts.Length);
        }
    }

    public class StorageItemStub : IStorageItem
    {
        public StorageItemStub(string basePath, string fullPath)
        {
            if (fullPath.Length > basePath.Length)
            {
                RelativePath = fullPath.Substring(basePath.Length + 1);
            }
            else
            {
                RelativePath = string.Empty;
            }

            RelativePath = RelativePath.Replace('\\', '/');
            Name = Path.GetFileName(fullPath);
            Extension = Path.GetExtension(fullPath);
        }

        public string Name { get; }

        public string Extension { get; }

        public string RelativePath { get; }

        public string FullPath { get; }

        public FamilyFolder FamilyFolder => FamilyFolder.None;
    }
}
