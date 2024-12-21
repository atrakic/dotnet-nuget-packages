using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GithubClient.Interfaces;
using Octokit;

namespace GithubClient
{
    public class GithubRepository : IGithubRepository
    {
        private readonly GitHubClient _githubClient;
        private readonly IGithubTokenGenerator _githubTokenGenerator;

        public GithubRepository(GitHubClient githubClient, IGithubTokenGenerator githubTokenGenerator)
        {
            _githubClient = githubClient;
            _githubTokenGenerator = githubTokenGenerator;
        }

        private async Task<Credentials> GetCredentials(string githubToken)
        {
            if (githubToken == null)
            {
                githubToken = await _githubTokenGenerator.GenerateToken();
            }
            return new Credentials(githubToken);
        }

        public async Task<IEnumerable<Branch>> GetBranches(string repository, string githubToken = null)
        {
            var tokenAuth = await GetCredentials(githubToken);
            _githubClient.Credentials = tokenAuth;

            return await _githubClient.Repository.Branch.GetAll(_githubClient.User.Current().Result.Login, repository);
        }

        public async Task<Branch> GetBranch(string repository, string branch, string githubToken = null)
        {
            var tokenAuth = await GetCredentials(githubToken);
            _githubClient.Credentials = tokenAuth;

            return await _githubClient.Repository.Branch.Get(_githubClient.User.Current().Result.Login, repository, branch);
        }

        public async Task<Repository> GetRepository(string repository, string githubToken = null)
        {
            var tokenAuth = await GetCredentials(githubToken);
            _githubClient.Credentials = tokenAuth;
            try
            {
                return await _githubClient.Repository.Get(_githubClient.User.Current().Result.Login, repository);
            }
            catch (NotFoundException)
            {
                return null;
            }
        }

        public async Task<TreeResponse> CreateTree(string repository, TreeResponse baseTree, List<NewTreeItem> treeItems, string githubToken = null)
        {
            var tokenAuth = await GetCredentials(githubToken);
            _githubClient.Credentials = tokenAuth;

            var tree = new NewTree();
            var containsFilesToDelete = treeItems.Any(t => t.Content == null);
            if (containsFilesToDelete)
            {
                // if the commit contains files to delete we have to make a new tree without a base sha
                // and add all existing files we want to keep
                // https://github.com/octokit/octokit.net/issues/1610
                var modifiedFiles = treeItems.Select(t => t.Path);
                baseTree.Tree.Where(t => t.Type != TreeType.Tree && modifiedFiles.All(a => a != t.Path)).Select(x => new NewTreeItem
                {
                    Path = x.Path,
                    Mode = x.Mode,
                    Type = x.Type.Value,
                    Sha = x.Sha
                }).ToList().ForEach(x => tree.Tree.Add(x));
            }
            else
            {
                tree.BaseTree = baseTree.Sha;
            }

            foreach (var treeItem in treeItems.Where(t => t.Content != null))
            {
                tree.Tree.Add(new NewTreeItem
                {
                    Content = treeItem.Content,
                    Mode = treeItem.Mode,
                    Path = treeItem.Path,
                    Type = treeItem.Type
                });
            }

            return await _githubClient.Git.Tree.Create(_githubClient.User.Current().Result.Login, repository, tree);
        }

        public async Task<TreeResponse> GetTree(string repository, string reference, bool recursive, string githubToken = null)
        {
            var tokenAuth = await GetCredentials(githubToken);
            _githubClient.Credentials = tokenAuth;
            if (recursive)
            {
                return await _githubClient.Git.Tree.GetRecursive(_githubClient.User.Current().Result.Login, repository, reference);
            }
            return await _githubClient.Git.Tree.Get(_githubClient.User.Current().Result.Login, repository, reference);
        }

        public async Task<Commit> CreateCommit(string repository, string tree, string message, string parent, string githubToken = null)
        {
            var tokenAuth = await GetCredentials(githubToken);
            _githubClient.Credentials = tokenAuth;

            var commit = new NewCommit(message, tree, parent);

            return await _githubClient.Git.Commit.Create(_githubClient.User.Current().Result.Login, repository, commit);
        }

        public async Task<Reference> CreateReference(string repository, string reference, string sha, string githubToken = null)
        {
            var tokenAuth = await GetCredentials(githubToken);
            _githubClient.Credentials = tokenAuth;

            var newReference = new NewReference(reference, sha);

            return await _githubClient.Git.Reference.Create(_githubClient.User.Current().Result.Login, repository, newReference);
        }

        public async Task<Reference> UpdateReference(string repository, string reference, string sha, string githubToken = null)
        {
            var tokenAuth = await GetCredentials(githubToken);
            _githubClient.Credentials = tokenAuth;

            return await _githubClient.Git.Reference.Update(_githubClient.User.Current().Result.Login, repository, reference, new ReferenceUpdate(sha));
        }

        public async Task<Reference> GetReference(string repository, string reference, string githubToken = null)
        {
            var tokenAuth = await GetCredentials(githubToken);
            _githubClient.Credentials = tokenAuth;

            return await _githubClient.Git.Reference.Get(_githubClient.User.Current().Result.Login, repository, reference);
        }

        public async Task<byte[]> GetRepositoryContent(string repository, string contentPath, string branch = null, string githubToken = null)
        {
            var tokenAuth = await GetCredentials(githubToken);
            _githubClient.Credentials = tokenAuth;

            try
            {
                if (branch == null)
                {
                    return await _githubClient.Repository.Content.GetRawContent(_githubClient.User.Current().Result.Login, repository,
                        contentPath);
                }

                return await _githubClient.Repository.Content.GetRawContentByRef(_githubClient.User.Current().Result.Login, repository, contentPath, branch);

            }
            catch (NotFoundException)
            {
                return null;
            }
        }

        public async Task<Repository> CreateRepository(string name, string githubToken = null)
        {
            var tokenAuth = await GetCredentials(githubToken);
            _githubClient.Credentials = tokenAuth;

            return await _githubClient.Repository.Create(_githubClient.User.Current().Result.Login, new NewRepository(name)
            {
                AutoInit = true,
                Private = true
            });
        }

        public async Task<IEnumerable<CommitStatus>> GetCommitStatus(string repository, string sha, string githubToken = null)
        {
            var tokenAuth = await GetCredentials(githubToken);
            _githubClient.Credentials = tokenAuth;

            return await _githubClient.Repository.Status.GetAll(_githubClient.User.Current().Result.Login, repository, sha);
        }
    }
}
