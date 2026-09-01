using System;
using System.Threading.Tasks;
using Arcadia.Core.Models;

namespace Arcadia.Media
{
    public class MediaDownloaderService
    {
        private readonly ImageCacheManager _cacheManager;

        public MediaDownloaderService()
        {
            _cacheManager = new ImageCacheManager();
        }

        public async Task<bool> DownloadMetadataAsync(Game game)
        {
            bool updated = false;

            // Steam publishes canonical artwork keyed by AppId. This avoids title
            // searches returning the wrong game (or a similarly named DLC/tool).
            if (game.LaunchType == LaunchType.Steam && !string.IsNullOrWhiteSpace(game.LauncherId))
            {
                string imageUrl = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{game.LauncherId}/library_600x900_2x.jpg";
                bool needsArtwork = string.IsNullOrWhiteSpace(game.BoxArtPath)
                    || !System.IO.File.Exists(game.BoxArtPath)
                    || game.BoxArtPath.Contains("picsum", StringComparison.OrdinalIgnoreCase);

                if (needsArtwork)
                {
                    string localPath = await _cacheManager.DownloadAndCacheImageAsync(game.Id, imageUrl);
                    if (!string.IsNullOrEmpty(localPath))
                    {
                        game.BoxArtPath = localPath;
                        updated = true;
                    }
                }
            }

            return updated;
        }
    }
}
