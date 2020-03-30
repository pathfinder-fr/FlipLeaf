using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using LibGit2Sharp;

namespace FlipLeaf.Services.Git
{
    public sealed class GitRepository : IDisposable
    {
        private readonly Repository _repository;
        private readonly object _gitLock;

        public GitRepository(Repository repository, object gitLock)
        {
            _repository = repository;
            _gitLock = gitLock;
        }

        public Repository Inner => _repository;

        public void Dispose()
        {
            _repository.Dispose();
            Monitor.Exit(_gitLock);
        }
    }
}
