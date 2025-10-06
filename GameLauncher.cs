using System;
using System.Diagnostics;
using System.IO;
using Arcadia.Core.Models;

namespace Arcadia.Core.Services
{
    /// <summary>
    /// Handles launching games, emulators, and PC launchers
    /// </summary>
    public class GameLauncher
    {
        private readonly GameDatabase _gameDatabase;

        public GameLauncher(GameDatabase gameDatabase)
        {
            _gameDatabase = gameDatabase;
        }

        /// <summary>
        /// Launch a game by its ID
        /// </summary>
        public bool LaunchGame(string gameId)
        {
            var game = _gameDatabase.GetGame(gameId);
            if (game == null)
            {
                throw new ArgumentException($"Game with ID {gameId} not found");
            }

            return LaunchGame(game);
        }

        /// <summary>
        /// Launch a game
        /// </summary>
        public bool LaunchGame(Game game)
        {
            try
            {
                switch (game.LaunchType)
                {
                    case LaunchType.Emulator:
                        return LaunchEmulator(game);
                    
                    case LaunchType.Steam:
                        return LaunchSteamGame(game);
                    
                    case LaunchType.GOG:
                        return LaunchGOGGame(game);
                    
                    case LaunchType.EpicGames:
                        return LaunchEpicGame(game);
                    
                    case LaunchType.TeknoParrot:
                        return LaunchTeknoParrotGame(game);
                    
                    case LaunchType.Standalone:
                        return LaunchStandaloneGame(game);
                    
                    default:
                        throw new NotSupportedException($"Launch type {game.LaunchType} is not supported");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error launching game {game.Title}: {ex.Message}");
                return false;
            }
            finally
            {
                // Update play statistics
                UpdatePlayStatistics(game);
            }
        }

        private bool LaunchEmulator(Game game)
        {
            if (string.IsNullOrEmpty(game.ExecutablePath) || string.IsNullOrEmpty(game.RomPath))
            {
                throw new InvalidOperationException("Emulator executable path or ROM path is not set");
            }

            if (!File.Exists(game.ExecutablePath))
            {
                throw new FileNotFoundException($"Emulator executable not found: {game.ExecutablePath}");
            }

            if (!File.Exists(game.RomPath))
            {
                throw new FileNotFoundException($"ROM file not found: {game.RomPath}");
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = game.ExecutablePath,
                Arguments = $"\"{game.RomPath}\" {game.CommandLineArgs}",
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(game.ExecutablePath) ?? string.Empty
            };

            Process.Start(startInfo);
            return true;
        }

        private bool LaunchSteamGame(Game game)
        {
            // Steam URI format: steam://rungameid/<appid>
            if (string.IsNullOrEmpty(game.LauncherId))
            {
                throw new InvalidOperationException("Steam App ID is not set");
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = $"steam://rungameid/{game.LauncherId}",
                UseShellExecute = true
            };

            Process.Start(startInfo);
            return true;
        }

        private bool LaunchGOGGame(Game game)
        {
            // GOG Galaxy URI format: goggalaxy://openGameView/<gameId>
            // Or direct executable launch
            if (!string.IsNullOrEmpty(game.LauncherId))
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = $"goggalaxy://openGameView/{game.LauncherId}",
                    UseShellExecute = true
                };

                Process.Start(startInfo);
                return true;
            }
            else if (!string.IsNullOrEmpty(game.ExecutablePath) && File.Exists(game.ExecutablePath))
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = game.ExecutablePath,
                    Arguments = game.CommandLineArgs,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(game.ExecutablePath) ?? string.Empty
                };

                Process.Start(startInfo);
                return true;
            }

            throw new InvalidOperationException("GOG game ID or executable path is not set");
        }

        private bool LaunchEpicGame(Game game)
        {
            // Epic Games URI format: com.epicgames.launcher://apps/<appName>?action=launch
            if (string.IsNullOrEmpty(game.LauncherId))
            {
                throw new InvalidOperationException("Epic Games App Name is not set");
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = $"com.epicgames.launcher://apps/{game.LauncherId}?action=launch",
                UseShellExecute = true
            };

            Process.Start(startInfo);
            return true;
        }

        private bool LaunchTeknoParrotGame(Game game)
        {
            if (string.IsNullOrEmpty(game.TeknoParrotProfilePath) || !File.Exists(game.TeknoParrotProfilePath))
            {
                throw new InvalidOperationException("TeknoParrot profile path is not set or does not exist");
            }

            // Find TeknoParrot executable (typically TeknoParrotUi.exe)
            string teknoParrotPath = FindTeknoParrotExecutable();
            if (string.IsNullOrEmpty(teknoParrotPath))
            {
                throw new FileNotFoundException("TeknoParrot executable not found");
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = teknoParrotPath,
                Arguments = $"--profile=\"{game.TeknoParrotProfilePath}\" {game.CommandLineArgs}",
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(teknoParrotPath) ?? string.Empty
            };

            Process.Start(startInfo);
            return true;
        }

        private bool LaunchStandaloneGame(Game game)
        {
            if (string.IsNullOrEmpty(game.ExecutablePath) || !File.Exists(game.ExecutablePath))
            {
                throw new InvalidOperationException("Standalone game executable path is not set or does not exist");
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = game.ExecutablePath,
                Arguments = game.CommandLineArgs,
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(game.ExecutablePath) ?? string.Empty
            };

            Process.Start(startInfo);
            return true;
        }

        private string FindTeknoParrotExecutable()
        {
            // Common TeknoParrot installation paths
            string[] searchPaths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "TeknoParrot", "TeknoParrotUi.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "TeknoParrot", "TeknoParrotUi.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "TeknoParrot", "TeknoParrotUi.exe"),
                @"C:\TeknoParrot\TeknoParrotUi.exe"
            };

            foreach (var path in searchPaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return string.Empty;
        }

        private void UpdatePlayStatistics(Game game)
        {
            game.TimesPlayed++;
            game.LastPlayed = DateTime.Now;
            _gameDatabase.UpdateGame(game);
        }
    }
}
