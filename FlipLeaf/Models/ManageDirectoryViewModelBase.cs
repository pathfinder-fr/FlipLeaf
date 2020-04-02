namespace FlipLeaf.Models
{
    public class ManageDirectoryViewModelBase
    {
        public ManageDirectoryViewModelBase(string path)
        {
            Path = path;
            PathParts = path.Split('/');
        }

        public string Path { get; set; }

        public string[] PathParts { get; set; }
    }
}
