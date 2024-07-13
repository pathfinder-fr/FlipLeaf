namespace FlipLeaf.Storage.Git
{
    public class GitCommit
    {
        public GitCommit(string sha, string message, DateTimeOffset authored)
        {
            Sha = sha;
            Message = message;
            Authored = authored;
        }

        public string Sha { get; }

        public string Message { get; }

        public DateTimeOffset Authored { get; }
    }
}
