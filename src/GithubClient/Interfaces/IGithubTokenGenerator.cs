using System.Threading.Tasks;

namespace GithubClient.Interfaces
{
    public interface IGithubTokenGenerator
    {
        Task<string> GenerateToken();
    }
}
