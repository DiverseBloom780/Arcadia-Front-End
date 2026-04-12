using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Arcadia.Core.Models;

namespace Arcadia.Launchers
{
    public class EpicGamesIntegration
    {
        private static readonly string ManifestPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Epic", "EpicGamesLauncher", "Data", "Manifests"
        );

        public List<Game> DetectInstalledGames()
        {
            var games = new List<Game>();
            if (!Directory.Exists(ManifestPath)) return games;

            var manifestFiles = Directory.GetFiles(ManifestPath, "*.item");
            foreach (var file in manifestFiles)
            {
                try
                {
                    var content = File.ReadAllText(file);
                    using var doc = JsonDocument.Parse(content);
                    var root = doc.RootElement;

                    string displayName = root.GetProperty("DisplayName").GetString() ?? "";
                    string appName = root.GetProperty("AppName").GetString() ?? "";
                    string installLocation = root.GetProperty("InstallLocation").GetString() ?? "";

                    if (string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(appName)) continue;

                    games.Add(new Game
                    {
                        Id = $"epic_{appName}",
                        Title = displayName,
                        Platform = "Epic Games",
                        LaunchType = LaunchType.EpicGames,
                        LauncherId = appName,
                        ExecutablePath = installLocation
                    });
                }
                catch
                {
                    // Skip invalid manifests
                }
            }

            return games;
        }
    }
}