using GithubClient.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Octokit;

namespace GithubClient
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddDevOpsGithubClient(this IServiceCollection services,
            string serviceName, int githubAppId, int githubAppInstallationId, string githubAppPrivateKey)
        {
            services.AddHttpClient();
            services.AddScoped<IGithubRepository, GithubRepository>();
            services.AddScoped<GitHubClient>(s => new GitHubClient(
                new ProductHeaderValue(serviceName)));
            services.AddScoped<IGithubClient, GithubClient>();
            services.AddScoped<IGithubTokenGenerator>(s =>
                new GithubTokenGenerator(serviceName, githubAppId, githubAppInstallationId, githubAppPrivateKey));
            return services;
        }
    }
}
