using System;

namespace FlipLeaf.Models
{
    public class ManageDirectoryViewModelBase
    {
        public ManageDirectoryViewModelBase(string path)
        {
            Path = path;
            PathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        }

        public string Path { get; set; }

        public string[] PathParts { get; set; }
    }
}
