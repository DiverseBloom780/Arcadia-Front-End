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
            => await DownloadAndCacheMediaAsync(gameId, imageUrl, "BoxArt");

        public async Task<string> DownloadAndCacheMediaAsync(string gameId, string mediaUrl, string mediaType)
        {
            if (string.IsNullOrEmpty(mediaUrl))
                return string.Empty;

            try
            {
                var uri = new Uri(mediaUrl);
                string extension = Path.GetExtension(uri.LocalPath);
                if (string.IsNullOrWhiteSpace(extension) || extension.Length > 5)
                    extension = mediaType.Equals("Video", StringComparison.OrdinalIgnoreCase) ? ".mp4" : ".jpg";

                var directory = Path.Combine(Path.GetDirectoryName(_cacheDirectory)!, mediaType);
                Directory.CreateDirectory(directory);
                var fileName = $"{gameId}_{mediaType.ToLowerInvariant()}{extension}";
                var localPath = Path.Combine(directory, fileName);

                if (File.Exists(localPath))
                {
                    return localPath; // Already cached
                }

                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Arcadia/1.0");
                var response = await client.GetAsync(mediaUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                if (imageBytes.Length == 0)
                    return string.Empty;
                await File.WriteAllBytesAsync(localPath, imageBytes);

                return localPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error caching media {mediaUrl}: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
