using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Arcadia.Core.Models;
using Microsoft.Win32;

namespace Arcadia.Launchers
{
    /// <summary>
    /// Detects and imports Steam games
    /// </summary>
    public class SteamIntegration
    {
        private string? _steamPath;
        private List<string> _libraryFolders = new List<string>();

        public SteamIntegration()
        {
            DetectSteamInstallation();
        }

        private void DetectSteamInstallation()
        {
            try
            {
                // Try to find Steam installation path from registry
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                if (key != null)
                {
                    _steamPath = key.GetValue("SteamPath") as string;
                }

                // Fallback to common installation paths
                if (string.IsNullOrEmpty(_steamPath))
                {
                    string[] commonPaths = new[]
                    {
                        @"C:\Program Files (x86)\Steam",
                        @"C:\Program Files\Steam",
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam")
                    };

                    foreach (var path in commonPaths)
                    {
                        if (Directory.Exists(path) && File.Exists(Path.Combine(path, "steam.exe")))
                        {
                            _steamPath = path;
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(_steamPath))
                {
                    DetectLibraryFolders();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting Steam installation: {ex.Message}");
            }
        }

        private void DetectLibraryFolders()
        {
            if (string.IsNullOrEmpty(_steamPath))
                return;

            _libraryFolders.Add(Path.Combine(_steamPath, "steamapps"));

            // Parse libraryfolders.vdf
            string libraryFoldersPath = Path.Combine(_steamPath, "steamapps", "libraryfolders.vdf");
            if (File.Exists(libraryFoldersPath))
            {
                try
                {
                    string content = File.ReadAllText(libraryFoldersPath);
                    var pathMatches = Regex.Matches(content, @"""path""\s+""([^""]+)""");
                    
                    foreach (Match match in pathMatches)
                    {
                        if (match.Groups.Count > 1)
                        {
                            string libraryPath = match.Groups[1].Value.Replace(@"\\", @"\");
                            string steamappsPath = Path.Combine(libraryPath, "steamapps");
                            if (Directory.Exists(steamappsPath) && !_libraryFolders.Contains(steamappsPath))
                            {
                                _libraryFolders.Add(steamappsPath);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing library folders: {ex.Message}");
                }
            }
        }

        public List<Game> DetectInstalledGames()
        {
            var games = new List<Game>();

            if (_libraryFolders.Count == 0)
            {
                Console.WriteLine("No Steam library folders detected");
                return games;
            }

            foreach (var libraryFolder in _libraryFolders)
            {
                try
                {
                    var acfFiles = Directory.GetFiles(libraryFolder, "appmanifest_*.acf");
                    
                    foreach (var acfFile in acfFiles)
                    {
                        try
                        {
                            var game = ParseSteamManifest(acfFile);
                            if (game != null)
                            {
                                games.Add(game);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing manifest {acfFile}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error scanning library folder {libraryFolder}: {ex.Message}");
                }
            }

            return games;
        }

        private Game? ParseSteamManifest(string manifestPath)
        {
            try
            {
                string content = File.ReadAllText(manifestPath);
                
                var appIdMatch = Regex.Match(content, @"""appid""\s+""(\d+)""");
                var nameMatch = Regex.Match(content, @"""name""\s+""([^""]+)""");
                var installDirMatch = Regex.Match(content, @"""installdir""\s+""([^""]+)""");

                if (!appIdMatch.Success || !nameMatch.Success)
                    return null;

                string appId = appIdMatch.Groups[1].Value;
                string name = nameMatch.Groups[1].Value;
                string installDir = installDirMatch.Success ? installDirMatch.Groups[1].Value : string.Empty;

                var game = new Game
                {
                    Id = $"steam_{appId}",
                    Title = name,
                    Platform = "Steam",
                    LaunchType = LaunchType.Steam,
                    LauncherId = appId
                };

                // Try to find the executable path
                if (!string.IsNullOrEmpty(installDir))
                {
                    string libraryFolder = Path.GetDirectoryName(manifestPath) ?? string.Empty;
                    string commonFolder = Path.Combine(libraryFolder, "common", installDir);
                    
                    if (Directory.Exists(commonFolder))
                    {
                        game.ExecutablePath = commonFolder;
                    }
                }

                return game;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing Steam manifest: {ex.Message}");
                return null;
            }
        }

        public bool IsSteamInstalled()
        {
            return !string.IsNullOrEmpty(_steamPath);
        }

        public string? GetSteamPath()
        {
            return _steamPath;
        }
    }
}
