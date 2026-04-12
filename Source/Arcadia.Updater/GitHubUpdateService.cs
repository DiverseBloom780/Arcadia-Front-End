using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace Arcadia.Updater
{
    public class GitHubUpdateService
    {
        private readonly GitHubClient _client;
        private readonly string _owner;
        private readonly string _repo;
        private readonly string _currentVersion;

        public GitHubUpdateService(string owner, string repo, string currentVersion)
        {
            _client = new GitHubClient(new ProductHeaderValue("Arcadia-Launcher"));
            _owner = owner;
            _repo = repo;
            _currentVersion = currentVersion;
        }

        public async Task<(bool UpdateAvailable, Release? LatestRelease)> CheckForUpdatesAsync()
        {
            try
            {
                var releases = await _client.Repository.Release.GetAll(_owner, _repo);
                var latest = releases[0];

                if (IsNewer(latest.TagName, _currentVersion))
                {
                    return (true, latest);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update check failed: {ex.Message}");
            }

            return (false, null);
        }

        private bool IsNewer(string latest, string current)
        {
            if (Version.TryParse(latest.TrimStart('v'), out Version? vLatest) &&
                Version.TryParse(current.TrimStart('v'), out Version? vCurrent))
            {
                return vLatest > vCurrent;
            }
            return false;
        }

        public string GetChangelog(Release release)
        {
            return release.Body ?? "No changelog provided.";
        }
    }
}