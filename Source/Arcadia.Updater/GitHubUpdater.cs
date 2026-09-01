using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace Arcadia.Updater
{
    public class GitHubUpdater
    {
        private readonly GitHubClient _client;
        private readonly string _owner;
        private readonly string _repository;
        private readonly string _currentVersion;

        public GitHubUpdater(string owner, string repository, string currentVersion)
        {
            _client = new GitHubClient(new ProductHeaderValue("Arcadia-Launcher"));
            _owner = owner;
            _repository = repository;
            _currentVersion = currentVersion;
        }

        public async Task<UpdateInfo?> CheckForUpdatesAsync()
        {
            try
            {
                var releases = await _client.Repository.Release.GetAll(_owner, _repository);
                if (releases.Count > 0)
                {
                    var latestRelease = releases[0];
                    if (IsNewerThanCurrent(latestRelease.TagName))
                    {
                        return new UpdateInfo
                        {
                            Version = latestRelease.TagName,
                            ReleaseNotes = latestRelease.Body,
                            DownloadUrl = latestRelease.Assets.Count > 0 ? latestRelease.Assets[0].BrowserDownloadUrl : latestRelease.HtmlUrl,
                            PublishedAt = latestRelease.PublishedAt ?? DateTimeOffset.Now
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking for updates: {ex.Message}");
            }

            return null;
        }

        private bool IsNewerThanCurrent(string latestVersion)
        {
            if (string.IsNullOrEmpty(_currentVersion)) return true;
            
            try
            {
                // Simple semantic version comparison
                var current = new Version(_currentVersion.TrimStart('v'));
                var latest = new Version(latestVersion.TrimStart('v'));
                return latest > current;
            }
            catch
            {
                // Fallback to string comparison if version format is non-standard
                return string.Compare(latestVersion, _currentVersion, StringComparison.OrdinalIgnoreCase) > 0;
            }
        }
    }

    public class UpdateInfo
    {
        public string Version { get; set; } = string.Empty;
        public string ReleaseNotes { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public DateTimeOffset PublishedAt { get; set; }
    }
}
