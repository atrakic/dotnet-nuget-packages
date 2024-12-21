using System.Threading.Tasks;
using Octokit;
using GithubClient.Interfaces;

namespace GithubClient
{
    public class GithubTokenGenerator : IGithubTokenGenerator
    {
        private readonly string _serviceName;
        private readonly int _githubAppId;
        private readonly int _githubAppInstallationId;
        private readonly string _githubAppPrivateKey;
        private AccessToken _accessToken;

        public GithubTokenGenerator(string serviceName, int githubAppId, int githubAppInstallationId, string githubAppPrivateKey)
        {
            _serviceName = serviceName;
            _githubAppId = githubAppId;
            _githubAppInstallationId = githubAppInstallationId;
            _githubAppPrivateKey = githubAppPrivateKey;
        }

        public async Task<string> GenerateToken()
        {
            if (_accessToken != null)
            {
                return _accessToken.Token;
            }

            var generator = new GitHubJwt.GitHubJwtFactory(
                new GitHubJwt.StringPrivateKeySource(_githubAppPrivateKey),
                new GitHubJwt.GitHubJwtFactoryOptions
                {
                    AppIntegrationId = _githubAppId, // The GitHub App Id
                    ExpirationSeconds = 600 // 10 minutes is the maximum time allowed
                }
            );

            var jwtToken = generator.CreateEncodedJwtToken();

            var appClient = new GitHubClient(new ProductHeaderValue(_serviceName))
            {
                Credentials = new Credentials(jwtToken, AuthenticationType.Bearer)
            };

            var token = await appClient.GitHubApps.CreateInstallationToken(_githubAppInstallationId);
            _accessToken = token;
            return token.Token;
        }
    }
}
