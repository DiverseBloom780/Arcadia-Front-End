using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Arcadia.Core.Models;
using Arcadia.Core.Data;
using Arcadia.Core.Helpers;

namespace Arcadia.Core.Services
{
    public class GameLauncher
    {
        private readonly Arcadia.Core.Data.GameDatabase _database;

        public GameLauncher(Arcadia.Core.Data.GameDatabase database)
        {
            _database = database;
        }

        public async Task<bool> LaunchGameAsync(Game game)
        {
            try
            {
                var startTime = DateTime.Now;
                ProcessStartInfo? startInfo = null;

                switch (game.LaunchType)
                {
                    case LaunchType.Steam:
                        startInfo = new ProcessStartInfo($"steam://rungameid/{game.LauncherId}") { UseShellExecute = true };
                        break;

                    case LaunchType.GOG:
                        startInfo = new ProcessStartInfo($"goggalaxy://openGameView/{game.LauncherId}") { UseShellExecute = true };
                        break;

                    case LaunchType.EpicGames:
                        startInfo = new ProcessStartInfo($"com.epicgames.launcher://apps/{game.LauncherId}?action=launch") { UseShellExecute = true };
                        break;

                    case LaunchType.Standalone:
                        if (string.IsNullOrEmpty(game.ExecutablePath) || !File.Exists(game.ExecutablePath))
                            throw new FileNotFoundException("Executable not found", game.ExecutablePath);

                        startInfo = new ProcessStartInfo(game.ExecutablePath)
                        {
                            Arguments = game.CommandLineArgs ?? "",
                            WorkingDirectory = Path.GetDirectoryName(game.ExecutablePath)
                        };
                        break;

                    case LaunchType.Emulator:
                        if (string.IsNullOrEmpty(game.EmulatorId))
                            throw new InvalidOperationException($"No emulator assigned to game: {game.Title}");

                        var emulator = _database.GetEmulator(game.EmulatorId);
                        if (emulator == null)
                            throw new Exception($"Emulator with ID '{game.EmulatorId}' not found in database.");

                        if (!File.Exists(emulator.ExecutablePath))
                            throw new FileNotFoundException("Emulator executable not found", emulator.ExecutablePath);

                        string resolvedArgs = CommandResolver.Resolve(emulator.CommandLineTemplate, game, emulator);

                        startInfo = new ProcessStartInfo(emulator.ExecutablePath)
                        {
                            Arguments = resolvedArgs,
                            WorkingDirectory = Path.GetDirectoryName(emulator.ExecutablePath)
                        };
                        break;

                    case LaunchType.TeknoParrot:
                        if (string.IsNullOrEmpty(game.ExecutablePath) || !File.Exists(game.ExecutablePath))
                            throw new FileNotFoundException("TeknoParrotUi.exe not found", game.ExecutablePath);

                        startInfo = new ProcessStartInfo(game.ExecutablePath)
                        {
                            Arguments = $"--profile=\"{game.TeknoParrotProfilePath}\"",
                            WorkingDirectory = Path.GetDirectoryName(game.ExecutablePath)
                        };
                        break;
                }

                if (startInfo != null)
                {
                    using var process = Process.Start(startInfo);
                    if (process == null) return false;

                    // Update basic stats immediately
                    game.LastPlayed = startTime;
                    game.TimesPlayed++;
                    _database.UpdateGame(game);

                    // Wait for the game to exit to track PlayTime
                    await process.WaitForExitAsync();
                    
                    game.PlayTime += (DateTime.Now - startTime).TotalMinutes;
                    _database.UpdateGame(game);

                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to launch game {game.Title}: {ex.Message}");
            }

            return false;
        }
    }
}