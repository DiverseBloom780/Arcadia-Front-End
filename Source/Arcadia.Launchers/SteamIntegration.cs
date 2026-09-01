using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using Arcadia.Core.Models;

namespace Arcadia.Launchers
{
    public class SteamIntegration
    {
        private string? _steamPath;

        public SteamIntegration()
        {
            DetectSteamInstallation();
        }

        private void DetectSteamInstallation()
        {
            try
            {
                _steamPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string;
                if (string.IsNullOrEmpty(_steamPath))
                {
                    _steamPath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", null) as string;
                }
                
                if (!string.IsNullOrEmpty(_steamPath))
                {
                    _steamPath = _steamPath.Replace("/", "\\");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting Steam installation: {ex.Message}");
            }
        }

        public List<Game> DetectInstalledGames()
        {
            var games = new List<Game>();
            if (string.IsNullOrEmpty(_steamPath) || !Directory.Exists(_steamPath))
                return games;

            try
            {
                // Find all library folders
                var libraryFolders = new List<string> { Path.Combine(_steamPath, "steamapps") };
                string vdfPath = Path.Combine(_steamPath, "steamapps", "libraryfolders.vdf");

                if (File.Exists(vdfPath))
                {
                    libraryFolders.AddRange(ParseLibraryFolders(vdfPath));
                }

                foreach (var folder in libraryFolders)
                {
                    if (Directory.Exists(folder))
                    {
                        var acfFiles = Directory.GetFiles(folder, "appmanifest_*.acf");
                        foreach (var acfFile in acfFiles)
                        {
                            var game = ParseSteamManifest(acfFile, folder);
                            if (game != null)
                            {
                                games.Add(game);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning Steam games: {ex.Message}");
            }

            return games;
        }

        private List<string> ParseLibraryFolders(string vdfPath)
        {
            var folders = new List<string>();
            try
            {
                string content = File.ReadAllText(vdfPath);
                // Simple parsing for library folders
                var lines = content.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains("\"path\""))
                    {
                        int lastQuote = line.LastIndexOf('"');
                        int secondLastQuote = line.LastIndexOf('"', lastQuote - 1);
                        if (secondLastQuote != -1 && lastQuote != -1)
                        {
                            string path = line.Substring(secondLastQuote + 1, lastQuote - secondLastQuote - 1);
                            path = path.Replace("\\\\", "\\");
                            folders.Add(Path.Combine(path, "steamapps"));
                        }
                    }
                }
            }
            catch { }
            return folders;
        }

        private Game? ParseSteamManifest(string acfPath, string libraryPath)
        {
            try
            {
                string content = File.ReadAllText(acfPath);
                string appId = GetValueFromAcf(content, "appid");
                string name = GetValueFromAcf(content, "name");
                string installDir = GetValueFromAcf(content, "installdir");

                if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(name))
                    return null;

                // Steam also stores runtimes, redistributables, shader caches, and
                // other tools as app manifests. They are not launchable games.
                string fullInstallPath = Path.Combine(libraryPath, "common", installDir);
                if (LauncherUtils.IsNonGameApplication(name, fullInstallPath))
                    return null;

                var game = new Game
                {
                    Id = $"steam_{appId}",
                    Title = name,
                    Platform = "Steam",
                    LaunchType = LaunchType.Steam,
                    LauncherId = appId
                };

                if (Directory.Exists(fullInstallPath))
                {
                    game.ExecutablePath = LauncherUtils.FindBestExecutable(fullInstallPath, name);
                }

                return game;
            }
            catch
            {
                return null;
            }
        }

        private string GetValueFromAcf(string content, string key)
        {
            string search = $"\"{key}\"";
            int index = content.IndexOf(search, StringComparison.OrdinalIgnoreCase);
            if (index == -1) return string.Empty;

            int firstQuoteAfterKey = content.IndexOf('"', index + search.Length);
            int secondQuoteAfterKey = content.IndexOf('"', firstQuoteAfterKey + 1);

            if (firstQuoteAfterKey != -1 && secondQuoteAfterKey != -1)
            {
                return content.Substring(firstQuoteAfterKey + 1, secondQuoteAfterKey - firstQuoteAfterKey - 1);
            }

            return string.Empty;
        }
    }
}
