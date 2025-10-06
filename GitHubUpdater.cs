using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Octokit;

namespace Arcadia.Updater
{
    /// <summary>
    /// Handles automatic updates from GitHub releases
    /// </summary>
    public class GitHubUpdater
    {
        private readonly GitHubClient _client;
        private readonly string _owner;
        private readonly string _repository;
        private readonly string _currentVersion;

        public GitHubUpdater(string owner, string repository, string currentVersion)
        {
            _owner = owner;
            _repository = repository;
            _currentVersion = currentVersion;
            _client = new GitHubClient(new ProductHeaderValue("Arcadia"));
        }

        /// <summary>
        /// Check if an update is available
        /// </summary>
        public async Task<UpdateInfo?> CheckForUpdatesAsync()
        {
            try
            {
                var releases = await _client.Repository.Release.GetAll(_owner, _repository);
                var latestRelease = releases.FirstOrDefault(r => !r.Prerelease && !r.Draft);

                if (latestRelease == null)
                {
                    Console.WriteLine("No releases found");
                    return null;
                }

                string latestVersion = latestRelease.TagName.TrimStart('v');
                
                if (IsNewerVersion(latestVersion, _currentVersion))
                {
                    return new UpdateInfo
                    {
                        Version = latestVersion,
                        ReleaseNotes = latestRelease.Body,
                        PublishedAt = latestRelease.PublishedAt?.DateTime ?? DateTime.Now,
                        DownloadUrl = GetDownloadUrl(latestRelease),
                        ReleaseName = latestRelease.Name
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking for updates: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Download and install an update
        /// </summary>
        public async Task<bool> DownloadAndInstallUpdateAsync(UpdateInfo updateInfo, IProgress<double>? progress = null)
        {
            if (string.IsNullOrEmpty(updateInfo.DownloadUrl))
            {
                Console.WriteLine("No download URL available");
                return false;
            }

            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), "ArcadiaUpdate");
                Directory.CreateDirectory(tempPath);

                string downloadPath = Path.Combine(tempPath, "ArcadiaUpdate.zip");

                // Download the update
                using var httpClient = new HttpClient();
                using var response = await httpClient.GetAsync(updateInfo.DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                long? totalBytes = response.Content.Headers.ContentLength;
                using var contentStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(downloadPath, System.IO.FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var buffer = new byte[8192];
                long totalRead = 0;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalRead += bytesRead;

                    if (totalBytes.HasValue && progress != null)
                    {
                        progress.Report((double)totalRead / totalBytes.Value * 100);
                    }
                }

                // Extract and apply the update
                string extractPath = Path.Combine(tempPath, "extracted");
                System.IO.Compression.ZipFile.ExtractToDirectory(downloadPath, extractPath);

                // Create update script
                string updateScript = CreateUpdateScript(extractPath);
                
                // Launch update script and exit
                Process.Start(new ProcessStartInfo
                {
                    FileName = updateScript,
                    UseShellExecute = true
                });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading and installing update: {ex.Message}");
                return false;
            }
        }

        private string GetDownloadUrl(Release release)
        {
            // Look for a Windows installer or zip file
            var asset = release.Assets.FirstOrDefault(a => 
                a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ||
                a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                a.Name.EndsWith(".msi", StringComparison.OrdinalIgnoreCase));

            return asset?.BrowserDownloadUrl ?? string.Empty;
        }

        private bool IsNewerVersion(string latestVersion, string currentVersion)
        {
            try
            {
                var latest = new Version(latestVersion);
                var current = new Version(currentVersion);
                return latest > current;
            }
            catch
            {
                // If version parsing fails, compare strings
                return string.Compare(latestVersion, currentVersion, StringComparison.OrdinalIgnoreCase) > 0;
            }
        }

        private string CreateUpdateScript(string extractPath)
        {
            string scriptPath = Path.Combine(Path.GetTempPath(), "ArcadiaUpdate.bat");
            string currentPath = AppDomain.CurrentDomain.BaseDirectory;

            string script = $@"
@echo off
echo Updating Arcadia...
timeout /t 3 /nobreak > nul

echo Copying new files...
xcopy /E /Y ""{extractPath}\*"" ""{currentPath}""

echo Update complete!
timeout /t 2 /nobreak > nul

echo Restarting Arcadia...
start """" ""{Path.Combine(currentPath, "Arcadia.exe")}""

del ""%~f0""
";

            File.WriteAllText(scriptPath, script);
            return scriptPath;
        }

        /// <summary>
        /// Get the changelog for a specific version
        /// </summary>
        public async Task<string> GetChangelogAsync(string version)
        {
            try
            {
                var releases = await _client.Repository.Release.GetAll(_owner, _repository);
                var release = releases.FirstOrDefault(r => r.TagName.TrimStart('v') == version);

                return release?.Body ?? "No changelog available";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching changelog: {ex.Message}");
                return "Error fetching changelog";
            }
        }
    }

    public class UpdateInfo
    {
        public string Version { get; set; } = string.Empty;
        public string ReleaseNotes { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
        public string ReleaseName { get; set; } = string.Empty;
    }
}
