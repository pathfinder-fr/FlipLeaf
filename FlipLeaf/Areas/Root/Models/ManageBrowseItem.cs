using System;

namespace FlipLeaf.Areas.Root.Models
{
    public class ManageBrowseItem
    {
        public bool IsDirectory { get; set; }

        public ItemPath Path { get; set; }

        public DateTimeOffset? LastUpdate { get; set; }

        public ManageBrowseItem WithCommit(Services.Git.Commit commit)
        {
            if (commit != null)
            {
                LastUpdate = commit.Authored;
            }

            return this;
        }
    }
}
