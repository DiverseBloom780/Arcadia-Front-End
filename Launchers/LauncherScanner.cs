using System;
using System.Collections.Generic;
using Microsoft.Win32;
using System.IO;

namespace Arcadia.Launchers {
    public static class LauncherScanner {
        public static List<string> ScanSteamGames() {
            var games = new List<string>();
            var steamPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null)?.ToString();
            if (steamPath == null) return games;

            var libraryPath = Path.Combine(steamPath, "steamapps");
            foreach (var file in Directory.GetFiles(libraryPath, "*.acf")) {
                var title = File.ReadAllText(file).Split("\n")[2].Split('"')[3]; // crude title parse
                games.Add(title);
            }
            return games;
        }

        public static List<string> ScanEpicGames() {
            var games = new List<string>();
            var epicPath = @"C:\ProgramData\Epic\EpicGamesLauncher\Data\Manifests";
            if (!Directory.Exists(epicPath)) return games;

            foreach (var file in Directory.GetFiles(epicPath, "*.item")) {
                var title = File.ReadAllText(file).Split("\"DisplayName\":\"")[1].Split("\"")[0];
                games.Add(title);
            }
            return games;
        }

        public static List<string> ScanGOGGames() {
            var games = new List<string>();
            var gogPath = @"C:\Program Files (x86)\GOG Galaxy\Games";
            if (!Directory.Exists(gogPath)) return games;

            foreach (var dir in Directory.GetDirectories(gogPath)) {
                games.Add(Path.GetFileName(dir));
            }
            return games;
        }
    }
}
