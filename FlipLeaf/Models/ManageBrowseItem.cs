using System;

namespace FlipLeaf.Models
{
    public class ManageBrowseItem
    {
        public ManageBrowseItem(Storage.IItemPath path, bool isDirectory)
        {
            Path = path;
            IsDirectory = isDirectory;
        }

        public bool IsDirectory { get; }

        public Storage.IItemPath Path { get; }

        public DateTimeOffset? LastUpdate { get; set; }

        public ManageBrowseItem WithCommit(LibGit2Sharp.Commit commit)
        {
            if (commit != null)
            {
                LastUpdate = commit.Author.When;
            }

            return this;
        }
    }
}
