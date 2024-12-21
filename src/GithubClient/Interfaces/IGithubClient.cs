using System.Collections.Generic;
using System.Threading.Tasks;
using GithubClient.Model;
using Octokit;

namespace GithubClient.Interfaces
{
    public interface IGithubClient
    {
        Task<Commit> CreateCommitAndPush(string repository, string branchName,
            IEnumerable<RepositoryFile> files, string message, bool newBranch, string githubToken = null);
        Task<string> GetDefaultBranch(string repository, string githubToken = null);
        Task<IEnumerable<CommitStatus>> GetCommitStatus(string repository, string sha, string githubToken = null);
        Task<Repository> GetRepository(string repository, string githubToken = null);
        Task<string> GetFileContent(string repository, string path, string branch = null, string githubToken = null);
        Task<IEnumerable<Branch>> GetBranches(string repository, string githubToken = null);
        Task<Repository> CreateRepository(string name, string githubToken = null);
        Task<Branch> GetBranch(string repository, string branch, string githubToken = null);
        Task<TreeResponse> GetTree(string repository, string reference, bool recursive, string githubToken = null);
        Task<IEnumerable<string>> GetFileNames(string repository, string branch, string path, string githubToken = null);
    }
}
