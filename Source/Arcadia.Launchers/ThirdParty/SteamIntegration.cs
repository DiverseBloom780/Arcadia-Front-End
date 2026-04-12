using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Arcadia.Core.Models;
using Microsoft.Win32;

namespace Arcadia.Launchers
{
    public class SteamIntegration
    {
        private readonly string? _steamPath;

        public SteamIntegration()
        {
            _steamPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string;
        }

        public List<Game> DetectInstalledGames()
        {
            var games = new List<Game>();
            if (string.IsNullOrEmpty(_steamPath) || !Directory.Exists(_steamPath)) return games;

            var libraryFolders = GetLibraryFolders();
            foreach (var library in libraryFolders)
            {
                var commonPath = Path.Combine(library, "steamapps");
                if (!Directory.Exists(commonPath)) continue;

                var manifestFiles = Directory.GetFiles(commonPath, "appmanifest_*.acf");
                foreach (var manifest in manifestFiles)
                {
                    var game = ParseManifest(manifest);
                    if (game != null) games.Add(game);
                }
            }
            return games;
        }

        private List<string> GetLibraryFolders()
        {
            var folders = new List<string> { _steamPath! };
            var vdfPath = Path.Combine(_steamPath!, "steamapps", "libraryfolders.vdf");

            if (File.Exists(vdfPath))
            {
                var content = File.ReadAllText(vdfPath);
                var matches = Regex.Matches(content, "\"path\"\\s+\"(.+?)\"");
                foreach (Match match in matches)
                {
                    var path = match.Groups[1].Value.Replace("\\\\", "\\");
                    if (Directory.Exists(path)) folders.Add(path);
                }
            }
            return folders.Distinct().ToList();
        }

        private Game? ParseManifest(string path)
        {
            try
            {
                var content = File.ReadAllText(path);
                var appId = GetVdfValue(content, "appid");
                var name = GetVdfValue(content, "name");

                if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(name)) return null;

                return new Game
                {
                    Id = $"steam_{appId}",
                    Title = name,
                    Platform = "Steam",
                    LaunchType = LaunchType.Steam,
                    LauncherId = appId
                };
            }
            catch { return null; }
        }

        private string? GetVdfValue(string content, string key)
        {
            var match = Regex.Match(content, $"\"{key}\"\\s+\"(.+?)\"");
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}