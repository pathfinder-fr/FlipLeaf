using System;

namespace FlipLeaf.Models
{
    public class ManageBrowseItem
    {
        public bool IsDirectory { get; set; }

        public ItemPath Path { get; set; }

        public DateTimeOffset? LastUpdate { get; set; }

        public ManageBrowseItem WithCommit(Services.Git.GitCommit commit)
        {
            if (commit != null)
            {
                LastUpdate = commit.Authored;
            }

            return this;
        }
    }
}
