using System;
using System.IO;
using System.Net;

namespace Arcadia.Media {
    public static class MediaFetcher {
        public static void FetchIfMissing(string assetType, string gameName, string targetPath) {
            if (File.Exists(targetPath)) return;

            string url = $"https://arcadia-assets.net/{assetType}/{Uri.EscapeDataString(gameName)}.png";
            try {
                new WebClient().DownloadFile(url, targetPath);
                Console.WriteLine($"Fetched {assetType} for {gameName}");
            } catch {
                Console.WriteLine($"Failed to fetch {assetType} for {gameName}");
            }
        }
    }
}
