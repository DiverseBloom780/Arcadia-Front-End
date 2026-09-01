using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Arcadia.Media
{
    public class ImageCacheManager
    {
        private readonly string _cacheDirectory;

        public ImageCacheManager()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _cacheDirectory = Path.Combine(appData, "Arcadia", "Media", "BoxArt");
            
            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
            }
        }

        public async Task<string> DownloadAndCacheImageAsync(string gameId, string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return string.Empty;

            try
            {
                var fileName = $"{gameId}_{Path.GetFileName(new Uri(imageUrl).LocalPath)}";
                var localPath = Path.Combine(_cacheDirectory, fileName);

                if (File.Exists(localPath))
                {
                    return localPath; // Already cached
                }

                using var client = new HttpClient();
                var imageBytes = await client.GetByteArrayAsync(imageUrl);
                await File.WriteAllBytesAsync(localPath, imageBytes);

                return localPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error caching image {imageUrl}: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
