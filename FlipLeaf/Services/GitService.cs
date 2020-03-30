using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using FlipLeaf.Services.Git;
using LibGit2Sharp;

namespace FlipLeaf.Services
{
    public interface IGitService
    {
        GitRepository OpenRepository();

        void SetLastCommit<T>(string parent, IDictionary<string, T> items, Action<T, DateTimeOffset> setLastUpdate);

        IEnumerable<Git.GitCommit> LogFile(string file, int count = 30);

        IEnumerable<Git.GitCommit> LogFile(GitRepository repo, string file, int count = 30);

        void PullPush(IUser merger);

        void Commit(IUser author, IUser committer, string path, string comment);
    }

    public class GitService : IGitService
    {
        private static readonly object _gitLock = new object();

        private readonly FlipLeafSettings _settings;

        /// <summary>
        /// Is set to true when the repository has been verified and created if required.
        /// </summary>
        private bool _repoIsValid;

        public GitService(FlipLeafSettings settings)
        {
            _settings = settings;
        }

        public GitRepository OpenRepository()
        {
            Monitor.Enter(_gitLock);
            try
            {
                return new GitRepository(OpenRepositoryCore(), _gitLock);
            }
            catch
            {
                Monitor.Exit(_gitLock);
                throw;
            }
        }

        public void SetLastCommit<T>(string parent, IDictionary<string, T> items, Action<T, DateTimeOffset> setLastUpdate)
        {
            var parentParts = parent.Split('/', StringSplitOptions.RemoveEmptyEntries);

            lock (_gitLock)
            {
                using var repo = OpenRepositoryCore();

                Tree newestTree = null;
                LibGit2Sharp.Commit newestCommit = null;

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

                    if (newestTree != null)
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
                                setLastUpdate(item, newestCommit.Author.When);
                                items.Remove(newestEntry.Name);
                            }
                            else if (previousEntry.TargetType == TreeEntryTargetType.Blob && newestEntry.TargetType == TreeEntryTargetType.Blob)
                            {
                                // compare
                                var previousBlob = previousEntry.Target as Blob;
                                var newestBlob = newestEntry.Target as Blob;
                                if (previousBlob.Sha != newestBlob.Sha)
                                {
                                    setLastUpdate(item, newestCommit.Author.When);
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

        public IEnumerable<Git.GitCommit> LogFile(string file, int count = 30)
        {
            lock (_gitLock)
            {
                using var repo = OpenRepositoryCore();
                return LogFile(repo, file, count);
            }
        }
        public IEnumerable<Git.GitCommit> LogFile(GitRepository repo, string file, int count = 30)
        {
            return LogFile(repo.Inner, file, count);
        }

        private IEnumerable<Git.GitCommit> LogFile(Repository repo, string file, int count = 30)
        {
            return repo.Commits
                .QueryBy(file)
                .Select(x => new Git.GitCommit { Sha = x.Commit.Sha, Message = x.Commit.Message, Authored = x.Commit.Author.When })
                .Take(count)
                .ToList();
        }

        public void PullPush(IUser merger)
        {
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

        public void Commit(IUser author, IUser committer, string path, string comment)
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
                    repo.Index.Add(path);
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
                if (!Directory.Exists(_settings.SourcePath))
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
