using System.Collections.Generic;

namespace FlipLeaf.Areas.Root.Models
{
    public class ManageBrowseViewModel : ManageDirectoryViewModelBase
    {
        public List<ManageBrowseItem> Directories { get; set; }

        public List<ManageBrowseItem> Files { get; set; }

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
