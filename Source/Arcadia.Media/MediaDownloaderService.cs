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
            // In a real application, this would query IGDB, SteamGridDB, or GiantBomb.
            // For now, we simulate an API call and use placeholder premium images.
            await Task.Delay(1000); // Simulate network latency

            bool updated = false;

            if (string.IsNullOrEmpty(game.Description))
            {
                game.Description = $"An epic adventure awaits in {game.Title}. Experience premium gameplay and stunning visuals.";
                updated = true;
            }

            if (string.IsNullOrEmpty(game.BoxArtPath))
            {
                // Placeholder dummy image URL for demo purposes
                string dummyUrl = $"https://picsum.photos/seed/{game.Id}/600/900";
                string localPath = await _cacheManager.DownloadAndCacheImageAsync(game.Id, dummyUrl);
                
                if (!string.IsNullOrEmpty(localPath))
                {
                    game.BoxArtPath = localPath;
                    updated = true;
                }
            }

            return updated;
        }
    }
}
