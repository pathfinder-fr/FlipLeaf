using FlipLeaf.Storage.Git;
using LibGit2Sharp;

namespace FlipLeaf.Storage
{
    public interface IGitRepository
    {
        void SetLastCommit<T>(string parent, IDictionary<string, T> items, Action<T, Commit> setLastCommit);

        IEnumerable<GitCommit> LogFile(string file, int count = 30);

        void PullPush(Website.IUser merger);

        void Commit(Website.IUser author, Website.IUser committer, string path, string? comment, bool remove = false);
    }

    public class GitRepository : IGitRepository
    {
        private static readonly object _gitLock = new object();

        private readonly FlipLeafSettings _settings;
        private bool _repoIsValid;

        public GitRepository(FlipLeafSettings settings)
        {
            _settings = settings;
        }

        public void SetLastCommit<T>(string parent, IDictionary<string, T> items, Action<T, Commit> setLastUpdate)
        {
            var parentParts = parent.Split('/', StringSplitOptions.RemoveEmptyEntries);

            lock (_gitLock)
            {
                using var repo = OpenRepositoryCore();

                Tree? newestTree = null;
                Commit? newestCommit = null;


                foreach (var commit in repo.Commits)
                {
                    if (items.Count == 0)
                    {
                        return;
                    }

                    var tree = commit.Tree;
                    for (int i = 0; i < parentParts.Length; i++)
                    {
                        var treeEntry = tree[parentParts[i]];
                        if (treeEntry == null) break;
                        tree = treeEntry.Target as Tree;
                        if (tree == null) break;
                    }

                    if (tree == null) continue;

                    if (newestTree != null && newestCommit != null)
                    {
                        foreach (var newestEntry in newestTree)
                        {
                            if (!items.TryGetValue(newestEntry.Name, out var item))
                            {
                                continue;
                            }

                            var previousEntry = tree[newestEntry.Name];
                            if (previousEntry == null)
                            {
                                // new file
                                setLastUpdate(item, newestCommit);
                                items.Remove(newestEntry.Name);
                            }
                            else if (previousEntry.TargetType == TreeEntryTargetType.Blob && newestEntry.TargetType == TreeEntryTargetType.Blob)
                            {
                                // compare
                                var previousBlob = previousEntry.Target as Blob;
                                var newestBlob = newestEntry.Target as Blob;
                                if (previousBlob != null && newestBlob != null && previousBlob.Sha != newestBlob.Sha)
                                {
                                    setLastUpdate(item, newestCommit);
                                    items.Remove(newestEntry.Name);
                                }
                            }
                        }
                    }

                    newestTree = tree;
                    newestCommit = commit;
                }
            }
        }

        public IEnumerable<GitCommit> LogFile(string file, int count = 30)
        {
            lock (_gitLock)
            {
                using var repo = OpenRepositoryCore();

                try
                {
                    return repo.Commits
                        .QueryBy(file)
                        .Take(count)
                        .Select(x => new GitCommit(x.Commit.Sha, x.Commit.Message, x.Commit.Author.When))
                        .ToList();
                }
                catch (KeyNotFoundException)
                {
                    // bug #1410
                    // https://github.com/libgit2/libgit2sharp/issues/1410
                    return Enumerable.Empty<GitCommit>();
                }
            }
        }

        public void PullPush(Website.IUser merger)
        {
            if (!_settings.GitEnabled)
            {
                return;
            }

            lock (_gitLock)
            {
                using (var repo = OpenRepositoryCore())
                {
                    if (!repo.Network.Remotes.Any())
                    {
                        return;
                    }

                    var pullOptions = new PullOptions
                    {
                        FetchOptions = new FetchOptions
                        {
                            CredentialsProvider = GitCredentialsHandler
                        }
                    };

                    Commands.Pull(repo, new Signature(merger.Name, merger.Email, DateTime.Now), pullOptions);

                    var pushOptions = new PushOptions
                    {
                        CredentialsProvider = GitCredentialsHandler
                    };

                    repo.Network.Push(repo.Branches[_settings.GitBranch], pushOptions);
                }
            }
        }

        public void Commit(Website.IUser author, Website.IUser committer, string path, string? comment, bool remove = false)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                comment = $"Edit {path}";
            }

            var authorSign = new Signature(author.Name, author.Email, DateTime.Now);
            committer ??= author;

            lock (_gitLock)
            {
                using (var repo = OpenRepositoryCore())
                {
                    if (!remove)
                    {
                        repo.Index.Add(path);
                    }
                    else
                    {
                        repo.Index.Remove(path);
                    }

                    repo.Index.Write();

                    try
                    {
                        var commit = repo.Commit(comment, authorSign, new Signature(committer.Name, committer.Email, DateTime.Now));
                    }
                    catch (EmptyCommitException)
                    {
                    }
                }
            }
        }

        private Credentials GitCredentialsHandler(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            if (string.IsNullOrEmpty(_settings.GitPassword))
            {
                return new DefaultCredentials();
            }

            return new UsernamePasswordCredentials()
            {
                Username = _settings.GitUsername,
                Password = _settings.GitPassword
            };
        }

        /// <summary>
        /// Opens the current git repository, create and initialize the repository if necessary.
        /// </summary>
        private Repository OpenRepositoryCore()
        {
            if (!_repoIsValid)
            {
                _repoIsValid = Repository.IsValid(_settings.SourcePath);
            }

            if (!_repoIsValid)
            {
                if (!System.IO.Directory.Exists(_settings.SourcePath))
                {
                    // should have been handled by UseFlipLeaf() in ApplicationBuilderExtensions...
                    throw new InvalidOperationException($"Invalid configuration : SourcePath is supposed to be set and design an existing directory on disk");
                }

                Repository.Init(_settings.SourcePath);

                _repoIsValid = true;
            }

            return new Repository(_settings.SourcePath);
        }
    }
}
