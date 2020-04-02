using System.Collections.Generic;

namespace FlipLeaf.Models
{
    public class ManageBrowseViewModel : ManageDirectoryViewModelBase
    {
        public ManageBrowseViewModel(string path, List<ManageBrowseItem> directories, List<ManageBrowseItem> files)
            : base(path)
        {
            Directories = directories;
            Files = files;
        }

        public List<ManageBrowseItem> Directories { get; }

        public List<ManageBrowseItem> Files { get; }

        public string CombineDirectory(string directoryName)
        {
            if (string.IsNullOrEmpty(Path))
            {
                return directoryName;
            }
            else
            {
                return Path + "/" + directoryName;
            }

        }

        public string CombineFile(string fileName)
        {
            if (string.IsNullOrEmpty(Path))
            {
                return fileName;
            }
            else
            {
                return Path + "/" + fileName;
            }
        }

    }
}
