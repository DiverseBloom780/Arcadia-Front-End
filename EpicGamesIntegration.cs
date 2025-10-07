using System;
using System.Collections.Generic;
using System.IO;
using Arcadia.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Arcadia.Launchers
{
    /// <summary>
    /// Detects and imports Epic Games Store games
    /// </summary>
    public class EpicGamesIntegration
    {
        private string? _epicGamesPath;
        private string? _manifestsPath;

        public EpicGamesIntegration()
        {
            DetectEpicGamesInstallation();
        }

        private void DetectEpicGamesInstallation()
        {
            try
            {
                // Epic Games Launcher stores manifests in ProgramData
                string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                _manifestsPath = Path.Combine(programData, "Epic", "EpicGamesLauncher", "Data", "Manifests");

                // Try to find Epic Games Launcher executable
                string[] commonPaths = new[]
                {
                    @"C:\Program Files (x86)\Epic Games\Launcher\Portal\Binaries\Win32\EpicGamesLauncher.exe",
                    @"C:\Program Files (x86)\Epic Games\Launcher\Portal\Binaries\Win64\EpicGamesLauncher.exe",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Epic Games", "Launcher", "Portal", "Binaries", "Win32", "EpicGamesLauncher.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Epic Games", "Launcher", "Portal", "Binaries", "Win64", "EpicGamesLauncher.exe")
                };

                foreach (var path in commonPaths)
                {
                    if (File.Exists(path))
                    {
                        _epicGamesPath = Path.GetDirectoryName(path);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting Epic Games installation: {ex.Message}");
            }
        }

        public List<Game> DetectInstalledGames()
        {
            var games = new List<Game>();

            if (string.IsNullOrEmpty(_manifestsPath) || !Directory.Exists(_manifestsPath))
            {
                Console.WriteLine("Epic Games manifests folder not found");
                return games;
            }

            try
            {
                var manifestFiles = Directory.GetFiles(_manifestsPath, "*.item");

                foreach (var manifestFile in manifestFiles)
                {
                    try
                    {
                        var game = ParseEpicManifest(manifestFile);
                        if (game != null)
                        {
                            games.Add(game);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing manifest {manifestFile}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning Epic Games manifests: {ex.Message}");
            }

            return games;
        }

        private Game? ParseEpicManifest(string manifestPath)
        {
            try
            {
                string content = File.ReadAllText(manifestPath);
                var manifest = JObject.Parse(content);

                string? displayName = manifest["DisplayName"]?.ToString();
                string? appName = manifest["AppName"]?.ToString();
                string? installLocation = manifest["InstallLocation"]?.ToString();
                string? launchExecutable = manifest["LaunchExecutable"]?.ToString();

                if (string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(appName))
                    return null;

                var game = new Game
                {
                    Id = $"epic_{appName}",
                    Title = displayName,
                    Platform = "Epic Games",
                    LaunchType = LaunchType.EpicGames,
                    LauncherId = appName
                };

                // Set executable path if available
                if (!string.IsNullOrEmpty(installLocation) && !string.IsNullOrEmpty(launchExecutable))
                {
                    string fullExePath = Path.Combine(installLocation, launchExecutable);
                    if (File.Exists(fullExePath))
                    {
                        game.ExecutablePath = fullExePath;
                    }
                }
                else if (!string.IsNullOrEmpty(installLocation))
                {
                    game.ExecutablePath = installLocation;
                }

                return game;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing Epic manifest: {ex.Message}");
                return null;
            }
        }

        public bool IsEpicGamesInstalled()
        {
            return !string.IsNullOrEmpty(_epicGamesPath) || (!string.IsNullOrEmpty(_manifestsPath) && Directory.Exists(_manifestsPath));
        }

        public string? GetEpicGamesPath()
        {
            return _epicGamesPath;
        }
    }
}
