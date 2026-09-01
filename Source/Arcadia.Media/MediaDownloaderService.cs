using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Arcadia.Core.Models;
using Arcadia.Core.Services;
using Newtonsoft.Json.Linq;

namespace Arcadia.Media
{
    public class MediaDownloaderService
    {
        private static readonly HttpClient Client = CreateClient();
        private readonly ImageCacheManager _cacheManager = new();
        private readonly MediaSettings _settings;

        public MediaDownloaderService(MediaSettings? settings = null)
        {
            _settings = settings ?? new MediaSettings
            {
                ScreenScraperDeveloperId = Environment.GetEnvironmentVariable("ARCADIA_SCREENSCRAPER_DEVID") ?? string.Empty,
                ScreenScraperDeveloperPassword = Environment.GetEnvironmentVariable("ARCADIA_SCREENSCRAPER_DEVPASSWORD") ?? string.Empty,
                ScreenScraperUsername = Environment.GetEnvironmentVariable("ARCADIA_SCREENSCRAPER_USER") ?? string.Empty,
                ScreenScraperPassword = Environment.GetEnvironmentVariable("ARCADIA_SCREENSCRAPER_PASSWORD") ?? string.Empty
            };
        }

        public async Task<bool> DownloadMetadataAsync(Game game)
        {
            try
            {
                return game.LaunchType == LaunchType.GOG
                    ? await DownloadGogMediaAsync(game)
                    : game.LaunchType == LaunchType.Steam
                        ? await DownloadSteamMediaAsync(game)
                        : await DownloadScreenScraperMediaAsync(game);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading media for {game.Title}: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> DownloadSteamMediaAsync(Game game)
        {
            if (string.IsNullOrWhiteSpace(game.LauncherId)) return false;
            string id = game.LauncherId;
            string? box = await DownloadFirstAsync(game, "BoxArt", $"https://cdn.cloudflare.steamstatic.com/steam/apps/{id}/library_600x900_2x.jpg", $"https://cdn.cloudflare.steamstatic.com/steam/apps/{id}/header.jpg");
            string? fan = await DownloadFirstAsync(game, "FanArt", $"https://cdn.cloudflare.steamstatic.com/steam/apps/{id}/library_hero.jpg", $"https://cdn.cloudflare.steamstatic.com/steam/apps/{id}/header.jpg");
            bool updated = AssignIfChanged(game.BoxArtPath, box, value => game.BoxArtPath = value) | AssignIfChanged(game.FanArtPath, fan, value => game.FanArtPath = value);
            if (string.IsNullOrWhiteSpace(game.LogoPath) && !string.IsNullOrWhiteSpace(box)) { game.LogoPath = box; updated = true; }
            return updated;
        }

        private async Task<bool> DownloadGogMediaAsync(Game game)
        {
            if (!long.TryParse(game.LauncherId, out var productId)) return await DownloadScreenScraperMediaAsync(game);
            using var response = await Client.GetAsync($"https://api.gog.com/products/{productId}");
            if (!response.IsSuccessStatusCode) return await DownloadScreenScraperMediaAsync(game);
            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            var images = json["images"];
            string? boxUrl = FindUrl(images, "boxart", "cover", "vertical", "icon");
            string? fanUrl = FindUrl(images, "background", "backdrop", "logo");
            bool updated = false;
            string? box = await DownloadFirstAsync(game, "BoxArt", boxUrl);
            string? fan = await DownloadFirstAsync(game, "FanArt", fanUrl);
            updated |= AssignIfChanged(game.BoxArtPath, box, value => game.BoxArtPath = value);
            updated |= AssignIfChanged(game.FanArtPath, fan, value => game.FanArtPath = value);
            if (string.IsNullOrWhiteSpace(game.LogoPath))
            {
                string? logo = await DownloadFirstAsync(game, "Logo", FindUrl(images, "logo", "icon"));
                updated |= AssignIfChanged(game.LogoPath, logo ?? box, value => game.LogoPath = value);
            }
            return updated;
        }

        private async Task<bool> DownloadScreenScraperMediaAsync(Game game)
        {
            if (string.IsNullOrWhiteSpace(_settings.ScreenScraperDeveloperId) || string.IsNullOrWhiteSpace(_settings.ScreenScraperDeveloperPassword))
                return false;

            string romName = Path.GetFileNameWithoutExtension(string.IsNullOrWhiteSpace(game.RomPath) ? game.Title : game.RomPath);
            string query = $"devid={Uri.EscapeDataString(_settings.ScreenScraperDeveloperId)}&devpassword={Uri.EscapeDataString(_settings.ScreenScraperDeveloperPassword)}&softname=Arcadia&output=json&romnom={Uri.EscapeDataString(romName)}";
            if (!string.IsNullOrWhiteSpace(_settings.ScreenScraperUsername))
                query += $"&ssid={Uri.EscapeDataString(_settings.ScreenScraperUsername)}&sspassword={Uri.EscapeDataString(_settings.ScreenScraperPassword)}";

            using var response = await Client.GetAsync($"https://api.screenscraper.fr/api2/jeuInfos.php?{query}");
            if (!response.IsSuccessStatusCode) return false;
            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            var media = json["response"]?["jeu"]?["medias"] as JArray;
            if (media == null) return false;

            bool updated = false;
            foreach (var item in media.OfType<JObject>())
            {
                string type = item["type"]?.ToString() ?? string.Empty;
                string? url = item["url"]?.ToString();
                if (string.IsNullOrWhiteSpace(url)) continue;
                string? path = await DownloadFirstAsync(game, type.Contains("video", StringComparison.OrdinalIgnoreCase) ? "Video" : type.Contains("logo", StringComparison.OrdinalIgnoreCase) ? "Logo" : type.Contains("fanart", StringComparison.OrdinalIgnoreCase) ? "FanArt" : "BoxArt", url);
                if (string.IsNullOrWhiteSpace(path)) continue;
                if (type.Contains("logo", StringComparison.OrdinalIgnoreCase)) updated |= AssignIfChanged(game.LogoPath, path, value => game.LogoPath = value);
                else if (type.Contains("fanart", StringComparison.OrdinalIgnoreCase) || type.Contains("screenshot", StringComparison.OrdinalIgnoreCase)) updated |= AssignIfChanged(game.FanArtPath, path, value => game.FanArtPath = value);
                else if (type.Contains("video", StringComparison.OrdinalIgnoreCase)) updated |= AssignIfChanged(game.VideoPreviewPath, path, value => game.VideoPreviewPath = value);
                else updated |= AssignIfChanged(game.BoxArtPath, path, value => game.BoxArtPath = value);
            }
            return updated;
        }

        private async Task<string?> DownloadFirstAsync(Game game, string mediaType, params string?[] urls)
        {
            foreach (var url in urls.Where(url => !string.IsNullOrWhiteSpace(url)))
            {
                string path = await _cacheManager.DownloadAndCacheMediaAsync(game.Id, url!, mediaType);
                if (!string.IsNullOrWhiteSpace(path)) return path;
            }
            return null;
        }

        private static string? FindUrl(JToken? token, params string[] preferredNames)
        {
            if (token == null) return null;
            if (token is JObject obj)
            {
                foreach (var name in preferredNames)
                    if (obj[name]?.Type == JTokenType.String)
                    {
                        string value = obj[name]!.ToString();
                        if (value.StartsWith("//", StringComparison.Ordinal)) return "https:" + value;
                        if (value.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return value;
                    }
                foreach (var child in obj.Properties()) { var result = FindUrl(child.Value, preferredNames); if (result != null) return result; }
            }
            else if (token is JArray array)
                foreach (var child in array) { var result = FindUrl(child, preferredNames); if (result != null) return result; }
            return null;
        }

        private static bool AssignIfChanged(string target, string? value, Action<string> setter)
        {
            if (string.IsNullOrWhiteSpace(value) || string.Equals(target, value, StringComparison.OrdinalIgnoreCase)) return false;
            setter(value); return true;
        }

        private static HttpClient CreateClient()
        {
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(25) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Arcadia/1.0");
            return client;
        }
    }
}
