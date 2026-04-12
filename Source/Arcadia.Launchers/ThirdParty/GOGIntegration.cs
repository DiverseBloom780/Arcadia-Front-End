using System;
using System.Collections.Generic;
using System.IO;
using Arcadia.Core.Models;
using Microsoft.Win32;

namespace Arcadia.Launchers
{
    public class GOGIntegration
    {
        public List<Game> DetectInstalledGames()
        {
            var games = new List<Game>();
            const string gogKeyPath = @"SOFTWARE\WOW6432Node\GOG.com\Games";
            
            using var key = Registry.LocalMachine.OpenSubKey(gogKeyPath);
            if (key == null) return games;

            foreach (var subKeyName in key.GetSubKeyNames())
            {
                using var subKey = key.OpenSubKey(subKeyName);
                if (subKey == null) continue;

                string? gameId = subKey.GetValue("gameID")?.ToString();
                string? title = subKey.GetValue("gameName")?.ToString();
                string? exePath = subKey.GetValue("exe")?.ToString();
                string? installPath = subKey.GetValue("path")?.ToString();

                if (string.IsNullOrEmpty(gameId) || string.IsNullOrEmpty(title)) continue;

                var game = new Game
                {
                    Id = $"gog_{gameId}",
                    Title = title,
                    Platform = "GOG",
                    LaunchType = LaunchType.GOG,
                    LauncherId = gameId
                };

                if (!string.IsNullOrEmpty(installPath) && !string.IsNullOrEmpty(exePath))
                {
                    string fullPath = Path.Combine(installPath, exePath);
                    if (File.Exists(fullPath)) game.ExecutablePath = fullPath;
                }

                games.Add(game);
            }

            return games;
        }
    }
}