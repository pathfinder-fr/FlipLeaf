using System;

namespace FlipLeaf.Services.Git
{
    public class GitCommit
    {
        public string Sha { get; set; }

        public string Message { get; set; }

        public DateTimeOffset Authored { get; set; }
    }
}
