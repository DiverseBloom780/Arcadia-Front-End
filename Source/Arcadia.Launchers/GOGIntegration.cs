using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using Arcadia.Core.Models;

namespace Arcadia.Launchers
{
    public class GOGIntegration
    {
        public List<Game> DetectInstalledGames()
        {
            var games = new List<Game>();

            try
            {
                // GOG games are listed in the registry
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\GOG.com\Games"))
                {
                    if (key != null)
                    {
                        foreach (string subkeyName in key.GetSubKeyNames())
                        {
                            using (var gameKey = key.OpenSubKey(subkeyName))
                            {
                                if (gameKey != null)
                                {
                                    string? title = gameKey.GetValue("gameName") as string;
                                    string? gameId = gameKey.GetValue("gameId") as string;
                                    string? exePath = gameKey.GetValue("exe") as string;
                                    string? installPath = gameKey.GetValue("path") as string;

                                    if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(gameId))
                                    {
                                        string resolvedExe = !string.IsNullOrEmpty(exePath) ? exePath : installPath ?? string.Empty;
                                        if (LauncherUtils.IsNonGameApplication(title, resolvedExe))
                                            continue;

                                        if (Directory.Exists(resolvedExe))
                                        {
                                            resolvedExe = LauncherUtils.FindBestExecutable(resolvedExe, title);
                                        }

                                        var game = new Game
                                        {
                                            Id = $"gog_{gameId}",
                                            Title = title,
                                            Platform = "GOG",
                                            LaunchType = LaunchType.GOG,
                                            LauncherId = gameId,
                                            ExecutablePath = resolvedExe
                                        };
                                        games.Add(game);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning GOG games: {ex.Message}");
            }

            return games;
        }
    }
}
