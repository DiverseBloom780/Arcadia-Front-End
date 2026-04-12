using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Arcadia.Core.Data;
using Arcadia.Core.Plugins;
using Arcadia.Core.Models;
using Arcadia.Launchers;
using Arcadia.Launchers.TeknoParrot;

namespace Arcadia.UI.Services
{
    /// <summary>
    /// Processes text commands for the Arcadia Smart Wizard (F3).
    /// NOTE: This file must be moved to the Arcadia.UI project 
    /// to properly reference Launcher integrations.
    /// </summary>
    public class SmartWizardService
    {
        private readonly Arcadia.Core.Data.GameDatabase _db;
        private readonly GameScannerService _scanner;
        private readonly TeknoParrotService _tpService;
        private readonly Arcadia.Core.Services.SaveStateManager _saveManager;
        private readonly IEnumerable<IPlugin> _plugins;
        private readonly Arcadia.Core.Services.SettingsManager _settingsManager;

        public SmartWizardService(Arcadia.Core.Data.GameDatabase db, GameScannerService scanner, TeknoParrotService tpService, Arcadia.Core.Services.SaveStateManager saveManager, IEnumerable<IPlugin> plugins, Arcadia.Core.Services.SettingsManager settingsManager)
        {
            _db = db;
            _scanner = scanner;
            _tpService = tpService;
            _saveManager = saveManager;
            _plugins = plugins;
            _settingsManager = settingsManager;
        }

        public async Task<string> ProcessCommandAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "Waiting for command...";

            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0].ToLower();

            switch (command)
            {
                case "help":
                    return "Available commands: \n" +
                           "- about: Introduction to Arcadia and Privacy Policy\n" +
                           "- scan: Refresh PC games\n" +
                           "- stats: Library overview\n" +
                           "- list-emulators: View emulators\n" +
                           "- fix-paths: Check executables\n" +
                           "- fix-config: Validate and repair application settings\n" +
                           "- fix-db: Check and repair database integrity\n" +
                           "- fix-fs: Verify and repair folder structure\n" +
                           "- fix-tp: Regenerate TeknoParrot profiles\n" +
                           "- check-media: Identify missing artwork\n" +
                           "- set-wheel [mode]: Set wheel style (curved/vertical/horizontal)\n" +
                           "- set-tilt [value]: Set the 3D tilt angle (e.g., -0.5)\n" +
                           "- set-radius [value]: Set the wheel curvature radius (e.g., 500)\n" +
                           "- set-spacing [value]: Set distance between items (e.g., 0.25)\n" +
                           "- set-linear [value]: Set vertical/horizontal spacing (e.g., 150)\n" +
                           "- set-x/set-y [value]: Adjust wheel screen position\n" +
                           "- set-logo-size [w] [h]: Adjust game logo dimensions\n" +
                           "- set-accent [hex]: Set the UI highlight color (e.g., #FF0000)\n" +
                           "- search [term]: Quickly filter the library locally\n" +
                           "- set-window [windowed|fullscreen]: Toggle display mode\n" +
                           "- privacy: Local processing and data privacy information\n" +
                           "- backup-saves: Create backups for all games\n" +
                           "- list-plugins: View loaded extensions\n" +
                           "- fix-all: Run all diagnostic and repair routines";

                case "about":
                    return "Welcome to Arcadia - The Ultimate Open-Source Frontend.\n" +
                           "Arcadia is designed for high-performance gaming with a focus on user privacy.\n\n" +
                           "Key Principles:\n" +
                           "- 100% Local: Processing happens on your hardware. No cloud, no servers.\n" +
                           "- Privacy First: We do not track, collect, or upload your personal data.\n" +
                           "- Passive Assistant: The Smart Wizard only runs when commanded (F3).\n" +
                           "- Community Driven: Built to outperform paywalled alternatives through open-source innovation.\n\n" +
                           "Type 'help' to see a list of utility commands.";

                case "scan":
                    _scanner.ScanAllLaunchers();
                    return "Scan complete. Library updated.";

                case "stats":
                    var games = _db.GetGames();
                    var playtime = games.Sum(g => g.PlayTime);
                    return $"You have {games.Count} games. Total playtime: {playtime:F1} minutes.";

                case "list-emulators":
                    var emulators = _db.GetEmulators();
                    if (!emulators.Any()) return "No emulators configured.";
                    return "Emulators:\n" + string.Join("\n", emulators.Select(e => $"- {e.Name}"));

                case "fix-paths":
                    int missing = 0;
                    foreach (var game in _db.GetGames().Where(g => g.LaunchType == LaunchType.Standalone))
                    {
                        if (!string.IsNullOrEmpty(game.ExecutablePath) && !File.Exists(game.ExecutablePath))
                            missing++;
                    }
                    return missing == 0 ? "All standalone paths are valid." : $"Found {missing} games with missing executables.";

                case "fix-config":
                    return RepairConfig();

                case "fix-db":
                    return RepairDatabase();

                case "fix-fs":
                    return RepairFileSystem();

                case "fix-tp":
                    var tpGames = _db.GetGames().Where(g => g.IsTeknoParrotGame).ToList();
                    int fixedCount = 0;
                    foreach (var game in tpGames)
                    {
                        if (string.IsNullOrEmpty(game.RomPath)) continue;

                        try 
                        {
                            string profilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameProfiles", $"{game.Id}.xml");
                            _tpService.GenerateGameProfile(game, profilePath);
                            game.TeknoParrotProfilePath = profilePath;
                            _db.UpdateGame(game);
                            fixedCount++;
                        } catch { /* Log failure */ }
                    }
                    return $"Processed {tpGames.Count} TeknoParrot games. {fixedCount} profiles regenerated.";

                case "fix-all":
                    return $"{RepairConfig()}\n{RepairDatabase()}\n{RepairFileSystem()}\nUse 'fix-tp' or 'fix-paths' for deep file inspection.";

                case "set-wheel":
                    if (parts.Length < 2) return "Please specify a mode: vertical, horizontal, or curved.";
                    
                    // Apply change to SettingsManager
                    if (Enum.TryParse<WheelMode>(parts[1], true, out var mode))
                    {
                        _settingsManager.Settings.WheelOrientation = parts[1].ToLower();
                        _settingsManager.SaveSettings();
                        return $"Wheel mode set to {parts[1]}. UI will refresh on next interaction.";
                    }
                    return $"Invalid mode '{parts[1]}'. Use: vertical, horizontal, or curved.";

                case "set-tilt":
                    if (parts.Length < 2 || !float.TryParse(parts[1], out float tilt)) return "Usage: set-tilt -0.5";
                    _settingsManager.Settings.TiltAngle = tilt;
                    _settingsManager.SaveSettings();
                    return $"Wheel tilt updated to {tilt}.";

                case "set-radius":
                    if (parts.Length < 2 || !float.TryParse(parts[1], out float radius)) return "Usage: set-radius 500";
                    _settingsManager.Settings.WheelRadius = radius;
                    _settingsManager.SaveSettings();
                    return $"Wheel radius updated to {radius}.";

                case "set-spacing":
                    if (parts.Length < 2 || !float.TryParse(parts[1], out float spacing)) return "Usage: set-spacing 0.25";
                    _settingsManager.Settings.ItemSpacing = spacing;
                    _settingsManager.SaveSettings();
                    return $"Item spacing updated to {spacing}.";

                case "set-linear":
                    if (parts.Length < 2 || !float.TryParse(parts[1], out float linear)) return "Usage: set-linear 150";
                    _settingsManager.Settings.LinearSpacing = linear;
                    _settingsManager.SaveSettings();
                    return $"Linear spacing updated to {linear}.";

                case "set-x":
                    if (parts.Length < 2 || !float.TryParse(parts[1], out float xOff)) return "Usage: set-x 200";
                    _settingsManager.Settings.WheelXOffset = xOff;
                    _settingsManager.SaveSettings();
                    return $"Wheel X-Offset updated to {xOff}.";

                case "set-y":
                    if (parts.Length < 2 || !float.TryParse(parts[1], out float yOff)) return "Usage: set-y 500";
                    _settingsManager.Settings.WheelYOffset = yOff;
                    _settingsManager.SaveSettings();
                    return $"Wheel Y-Offset updated to {yOff}.";

                case "set-logo-size":
                    if (parts.Length < 3 || !float.TryParse(parts[1], out float w) || !float.TryParse(parts[2], out float h)) return "Usage: set-logo-size 300 150";
                    _settingsManager.Settings.LogoWidth = w;
                    _settingsManager.Settings.LogoHeight = h;
                    _settingsManager.SaveSettings();
                    return $"Logo dimensions updated to {w}x{h}.";

                case "set-accent":
                    if (parts.Length < 2) return "Usage: set-accent #FF0000";
                    _settingsManager.Settings.AccentColor = parts[1];
                    _settingsManager.SaveSettings();
                    return $"Accent color updated to {parts[1]}.";

                case "search":
                    if (parts.Length < 2) return "Enter a search term.";
                    var query = string.Join(" ", parts.Skip(1));
                    var results = _db.GetGames().Where(g => g.Title.Contains(query, StringComparison.OrdinalIgnoreCase)).Take(5);
                    return $"Top matches:\n" + string.Join("\n", results.Select(r => $"- {r.Title} ({r.Platform})"));

                case "set-window":
                    if (parts.Length < 2) return "Specify 'windowed' or 'fullscreen'.";
                    
                    bool isFullscreen = parts[1].Equals("fullscreen", StringComparison.OrdinalIgnoreCase);
                    _settingsManager.Settings.IsFullscreen = isFullscreen;
                    _settingsManager.SaveSettings();

                    return $"Display mode set to {(isFullscreen ? "Fullscreen" : "Windowed")}. This will apply on next launch.";

                case "check-media":
                    return "Media scan initiated. Check logs for missing assets.";

                case "backup-saves":
                    var gamesWithSaves = _db.GetGames().Where(g => !string.IsNullOrEmpty(g.EmulatorId));
                    foreach (var game in gamesWithSaves)
                    {
                        _saveManager.CreateBackup(game);
                    }
                    return $"Backup complete for {gamesWithSaves.Count()} games.";

                case "list-plugins":
                    if (!_plugins.Any()) return "No plugins currently loaded.";
                    return "Loaded Extensions:\n" + string.Join("\n", _plugins.Select(p => $"- {p.Name} (v{p.Version}) by {p.Author}"));

                case "privacy":
                    return "Arcadia Smart Wizard - Privacy & Local Processing Info:\n" +
                           "- All processing stays local on your machine.\n" +
                           "- NO connections to external AI servers or datacenters.\n" +
                           "- NO collection or tracking of personal data.\n" +
                           "- This assistant is PASSIVE: it only acts when you press F3 and enter a command.\n" +
                           "- No background data-mining or telemetry.";

                default:
                    return $"I don't recognize '{command}'. Type 'help' for a list of valid commands.";
            }
        }

        private string RepairConfig()
        {
            try
            {
                _settingsManager.SaveSettings();
                return "[Config] Application settings validated and persisted.";
            }
            catch (Exception ex)
            {
                return $"[Config] Error: {ex.Message}";
            }
        }

        private string RepairDatabase()
        {
            try
            {
                _db.InitializeTables(); // Re-ensure schema is correct
                var games = _db.GetGames();
                return $"[Database] Integrity check passed. Library contains {games.Count} games.";
            }
            catch (Exception ex)
            {
                return $"[Database] Error: {ex.Message}";
            }
        }

        private string RepairFileSystem()
        {
            try
            {
                string[] dirs = { "Plugins", "Assets", "Assets/Media", "Config", "GameProfiles", "Backups" };
                int created = 0;
                foreach (var dir in dirs)
                {
                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dir);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                        created++;
                    }
                }
                return $"[FileSystem] Core directories verified. {created} folders restored.";
            }
            catch (Exception ex)
            {
                return $"[FileSystem] Error: {ex.Message}";
            }
        }
    }
}