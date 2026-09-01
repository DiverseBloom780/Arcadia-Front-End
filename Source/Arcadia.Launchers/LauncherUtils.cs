using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Arcadia.Launchers
{
    public static class LauncherUtils
    {
        private static readonly string[] NonGameApplicationMarkers =
        {
            "common redistributables",
            "steamworks common redistributables",
            "steam linux runtime",
            "proton easyanticheat runtime",
            "steam shader precaching",
            "steam controller configurator",
            "steam runtime"
        };

        public static bool IsNonGameApplication(string? title, string? installPath = null)
        {
            string value = $"{title ?? string.Empty} {installPath ?? string.Empty}";
            return NonGameApplicationMarkers.Any(marker => value.Contains(marker, StringComparison.OrdinalIgnoreCase))
                || value.Contains("\\_commonredist\\", StringComparison.OrdinalIgnoreCase)
                || value.Contains("\\redist\\", StringComparison.OrdinalIgnoreCase);
        }

        public static string FindBestExecutable(string directory, string gameTitle)
        {
            try
            {
                if (!Directory.Exists(directory)) return directory;

                var files = Directory.GetFiles(directory, "*.exe", SearchOption.AllDirectories);
                if (files.Length == 0) return directory;
                if (files.Length == 1) return files[0];

                // Filter out common installers and utilities
                var blacklist = new[] { 
                    "setup", "install", "unins", "redist", "touch", "crash", "unityman", 
                    "dxsetup", "helper", "overlay", "social", "updater", "vcredist", "vcredist_x64", 
                    "vcredist_x86", "service", "dotnet", "physx", "steam", "easyanticheat",
                    "launcher", "brower", "crashpad", "repair", "config", "settings"
                };

                var candidates = files.Where(f => !blacklist.Any(b => Path.GetFileName(f).ToLower().Contains(b))).ToList();
                
                // Never select an installer/runtime when no game executable remains.
                // The launcher URI is still sufficient for Steam/GOG/Epic games.
                if (candidates.Count == 0)
                    return directory;
                
                if (candidates.Count == 1) return candidates[0];

                // Try to find one that matches the game title closely
                string cleanTitle = new string(gameTitle.Where(char.IsLetterOrDigit).ToArray()).ToLower();
                var bestMatch = candidates.FirstOrDefault(f => 
                {
                    string name = Path.GetFileNameWithoutExtension(f).ToLower();
                    string cleanName = new string(name.Where(char.IsLetterOrDigit).ToArray());
                    return cleanName.Contains(cleanTitle) || cleanTitle.Contains(cleanName);
                });

                if (bestMatch != null) return bestMatch;

                // Default to the largest file remaining (usually the actual game engine)
                return candidates.OrderByDescending(f => new FileInfo(f).Length).First();
            }
            catch
            {
                return directory;
            }
        }
    }
}
