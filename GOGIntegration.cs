using System;
using System.Collections.Generic;
using System.IO;
using Arcadia.Core.Models;
using Microsoft.Win32;

namespace Arcadia.Launchers
{
    /// <summary>
    /// Detects and imports GOG games
    /// </summary>
    public class GOGIntegration
    {
        private string? _gogGalaxyPath;

        public GOGIntegration()
        {
            DetectGOGGalaxyInstallation();
        }

        private void DetectGOGGalaxyInstallation()
        {
            try
            {
                // Try to find GOG Galaxy installation path from registry
                using var key = Registry.LocalMachine.OpenSubKey(@"Software\GOG.com\GalaxyClient\paths");
                if (key != null)
                {
                    _gogGalaxyPath = key.GetValue("client") as string;
                }

                // Fallback to common installation paths
                if (string.IsNullOrEmpty(_gogGalaxyPath))
                {
                    string[] commonPaths = new[]
                    {
                        @"C:\Program Files (x86)\GOG Galaxy",
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "GOG Galaxy"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "GOG Galaxy")
                    };

                    foreach (var path in commonPaths)
                    {
                        if (Directory.Exists(path) && File.Exists(Path.Combine(path, "GalaxyClient.exe")))
                        {
                            _gogGalaxyPath = path;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting GOG Galaxy installation: {ex.Message}");
            }
        }

        public List<Game> DetectInstalledGames()
        {
            var games = new List<Game>();

            try
            {
                // GOG games are registered in the Windows registry
                using var key = Registry.LocalMachine.OpenSubKey(@"Software\GOG.com\Games");
                if (key != null)
                {
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        try
                        {
                            using var gameKey = key.OpenSubKey(subKeyName);
                            if (gameKey != null)
                            {
                                var game = ParseGOGGameKey(gameKey, subKeyName);
                                if (game != null)
                                {
                                    games.Add(game);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing GOG game key {subKeyName}: {ex.Message}");
                        }
                    }
                }

                // Also check HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\GOG.com\Games for 32-bit games on 64-bit systems
                using var key32 = Registry.LocalMachine.OpenSubKey(@"Software\WOW6432Node\GOG.com\Games");
                if (key32 != null)
                {
                    foreach (var subKeyName in key32.GetSubKeyNames())
                    {
                        try
                        {
                            using var gameKey = key32.OpenSubKey(subKeyName);
                            if (gameKey != null)
                            {
                                var game = ParseGOGGameKey(gameKey, subKeyName);
                                if (game != null && !games.Exists(g => g.Id == game.Id))
                                {
                                    games.Add(game);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing GOG game key {subKeyName}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting GOG games: {ex.Message}");
            }

            return games;
        }

        private Game? ParseGOGGameKey(RegistryKey gameKey, string gameId)
        {
            try
            {
                string? gameName = gameKey.GetValue("gameName") as string;
                string? path = gameKey.GetValue("path") as string;
                string? exePath = gameKey.GetValue("exePath") as string;

                if (string.IsNullOrEmpty(gameName))
                    return null;

                var game = new Game
                {
                    Id = $"gog_{gameId}",
                    Title = gameName,
                    Platform = "GOG",
                    LaunchType = LaunchType.GOG,
                    LauncherId = gameId
                };

                // Set executable path if available
                if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(exePath))
                {
                    string fullExePath = Path.Combine(path, exePath);
                    if (File.Exists(fullExePath))
                    {
                        game.ExecutablePath = fullExePath;
                    }
                }
                else if (!string.IsNullOrEmpty(path))
                {
                    game.ExecutablePath = path;
                }

                return game;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing GOG game key: {ex.Message}");
                return null;
            }
        }

        public bool IsGOGGalaxyInstalled()
        {
            return !string.IsNullOrEmpty(_gogGalaxyPath);
        }

        public string? GetGOGGalaxyPath()
        {
            return _gogGalaxyPath;
        }
    }
}
