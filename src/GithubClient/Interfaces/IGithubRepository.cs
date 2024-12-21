using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace GithubClient.Interfaces
{
    public interface IGithubRepository
    {
        Task<IEnumerable<Branch>> GetBranches (string repository, string githubToken = null);
        Task<Branch> GetBranch(string repository, string branch, string githubToken = null);
        Task<Repository> GetRepository (string repository, string githubToken = null);
        Task<TreeResponse> CreateTree (string repository, TreeResponse baseTree,
            List<NewTreeItem> treeItems, string githubToken = null);
        Task<TreeResponse> GetTree(string repository, string reference, bool recursive, string githubToken = null);
        Task<Commit> CreateCommit(string repository, string tree,
            string message, string parent, string githubToken = null);
        Task<Reference> CreateReference (string repository, string reference, string sha, string githubToken = null);
        Task<Reference> UpdateReference (string repository, string reference, string sha, string githubToken = null);
        Task<Reference> GetReference (string repository, string reference, string githubToken = null);
        Task<byte[]> GetRepositoryContent(string repository, string contentPath, string branch = null, string githubToken = null);
        Task<Repository> CreateRepository(string name, string githubToken = null);
        Task<IEnumerable<CommitStatus>> GetCommitStatus(string repository, string sha, string githubToken = null);
    }
}
