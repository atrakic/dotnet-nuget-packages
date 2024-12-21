using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using GithubClient.Interfaces;
using GithubClient.Model;
using Octokit;

namespace GithubClient
{
    public class GithubClient : IGithubClient
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IGithubRepository _githubRepository;

        public GithubClient(IGithubRepository githubRepository)
        {
            _githubRepository = githubRepository;
        }

        public async Task<Repository> GetRepository(string repository, string githubToken = null)
        {
            return await _githubRepository.GetRepository(repository, githubToken);
        }

        public async Task<string> GetDefaultBranch(string repository, string githubToken = null)
        {
            var repo = await _githubRepository.GetRepository(repository, githubToken);
            return repo.DefaultBranch;
        }

        public async Task<Commit> CreateCommitAndPush(string repository, string branchName,
            IEnumerable<RepositoryFile> files, string message, bool newBranch, string githubToken = null)
        {
            // Figure out what to base the new commit on
            var parentBranch = await GetDefaultBranch(repository, githubToken);
            var parentReference = await _githubRepository.GetReference(repository, $"refs/heads/{parentBranch}", githubToken);

            // In order to create a new commit, the following sequence of calls is needed.
            // See https://stackoverflow.com/questions/61583403/edit-multiple-files-in-single-commit-with-github-api
            var treeItems = files.Select(file => new NewTreeItem
            {
                Content = file.Content,
                Mode = "100644", // This mode signifies that it is a file.
                Path = file.DestinationPath,
                Type = TreeType.Blob
            }).ToList();

            var containsDeletedFiles = files.Any(f => f.Content == null);
            var baseTree = await _githubRepository.GetTree(repository, parentReference.Object.Sha, containsDeletedFiles, githubToken);

            var tree = await _githubRepository.CreateTree(repository, baseTree, treeItems, githubToken);

            var commitResponse = await _githubRepository.CreateCommit(repository,
                tree.Sha, message, baseTree.Sha, githubToken);

            if (newBranch)
            {
                await _githubRepository.CreateReference(
                    repository,
                    $"refs/heads/{branchName}", commitResponse.Sha, githubToken);
            }
            else
            {
                await _githubRepository.UpdateReference(
                    repository,
                    $"refs/heads/{branchName}", commitResponse.Sha, githubToken);
            }

            return commitResponse;
        }

        public async Task<IEnumerable<CommitStatus>> GetCommitStatus(string repository, string sha, string githubToken = null)
        {
            return await _githubRepository.GetCommitStatus(repository, sha, githubToken);
        }

        public async Task<string> GetFileContent(string repository, string path, string branch = null, string githubToken = null)
        {
            var rawContent = await _githubRepository.GetRepositoryContent(repository, path, branch, githubToken);
            return rawContent == null ? null : Encoding.UTF8.GetString(rawContent);
        }

        public async Task<IEnumerable<Branch>> GetBranches(string repository, string githubToken = null)
        {
            return await _githubRepository.GetBranches(repository, githubToken);
        }

        public async Task<Branch> GetBranch(string repository, string branch, string githubToken = null)
        {
            return await _githubRepository.GetBranch(repository, branch, githubToken);
        }

        public async Task<Repository> CreateRepository(string name, string githubToken = null)
        {
            return await _githubRepository.CreateRepository(name, githubToken);
        }

        public async Task<TreeResponse> GetTree(string repository, string reference, bool recursive, string githubToken = null)
        {
            return await _githubRepository.GetTree(repository, reference, recursive, githubToken);
        }

        public async Task<IEnumerable<string>> GetFileNames(string repository, string branch, string path, string githubToken = null)
        {
            var reference = await _githubRepository.GetReference(repository, $"refs/heads/{branch}", githubToken);
            var tree = await _githubRepository.GetTree(repository, reference.Object.Sha, true, githubToken);

            if (path == "/" || path == "." || path == "./")
            {
                return tree.Tree
                    .Where(item => item.Type == TreeType.Blob && !item.Path.Contains('/'))
                    .Select(f => f.Path);
            }

            path = path.TrimStart('/');
            return tree.Tree
                .Where(item => item.Type == TreeType.Blob &&
                               item.Path.Contains('/') &&
                               item.Path.Substring(0, item.Path.LastIndexOf('/')) == path)
                .Select(f => f.Path.Substring(f.Path.LastIndexOf('/') + 1));
        }
    }
}
