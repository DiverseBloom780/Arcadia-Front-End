using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Arcadia.Core.Models;

namespace Arcadia.Launchers
{
    public class TeknoParrotIntegration
    {
        private const string TeknoParrotPath = "C:\\TeknoParrot"; // Default TeknoParrot installation path
        private const string GameProfilesPath = "C:\\TeknoParrot\\GameProfiles";
        private const string RomsPath = "C:\\TeknoParrot\\Roms";

        public bool IsTeknoParrotInstalled()
        {
            return Directory.Exists(TeknoParrotPath) && Directory.Exists(GameProfilesPath) && Directory.Exists(RomsPath);
        }

        public List<Game> DetectInstalledGames()
        {
            List<Game> detectedGames = new List<Game>();

            if (!IsTeknoParrotInstalled())
            {
                Console.WriteLine("TeknoParrot not found at expected path.");
                return detectedGames;
            }

            // Scan ROMs folder for games and generate profiles if missing
            foreach (string romFolder in Directory.GetDirectories(RomsPath))
            {
                string gameName = new DirectoryInfo(romFolder).Name;
                string profilePath = Path.Combine(GameProfilesPath, $"{gameName}.xml");

                if (!File.Exists(profilePath))
                {
                    Console.WriteLine($"Generating basic profile for {gameName}");
                    GenerateBasicGameProfile(gameName, profilePath);
                }

                // Read profile and create Game object
                Game game = ParseGameProfile(profilePath);
                if (game != null)
                {
                    detectedGames.Add(game);
                }
            }

            return detectedGames;
        }

        private void GenerateBasicGameProfile(string gameName, string profilePath)
        {
            // This is a simplified example. A real implementation would need more logic
            // to determine game type and specific settings based on gameName or other heuristics.
            // For now, it creates a basic profile structure.
            XDocument doc = new XDocument(
                new XElement("GameProfile",
                    new XElement("GameName", gameName),
                    new XElement("GamePath", Path.Combine(RomsPath, gameName, $"{gameName}.exe")), // Placeholder, needs actual game executable detection
                    new XElement("Launcher", "TeknoParrot"),
                    new XElement("InputProfile", "Default"),
                    new XElement("RomPath", Path.Combine(RomsPath, gameName))
                )
            );
            doc.Save(profilePath);
        }

        private Game ParseGameProfile(string profilePath)
        {
            try
            {
                XDocument doc = XDocument.Load(profilePath);
                XElement root = doc.Root;

                if (root == null || root.Name != "GameProfile") return null;

                string gameName = root.Element("GameName")?.Value;
                string gamePath = root.Element("GamePath")?.Value;
                string launcher = root.Element("Launcher")?.Value;
                string romPath = root.Element("RomPath")?.Value;

                if (string.IsNullOrEmpty(gameName) || string.IsNullOrEmpty(gamePath) || launcher != "TeknoParrot")
                {
                    return null;
                }

                return new Game
                {
                    Title = gameName,
                    Path = gamePath,
                    Platform = "Arcade (TeknoParrot)",
                    RomPath = romPath
                    // Populate other properties as needed from the XML
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing TeknoParrot profile {profilePath}: {ex.Message}");
                return null;
            }
        }

        public void AutoConfigureInput(Game game)
        {
            Console.WriteLine($"Auto-configuring input for {game.Title}...");
            // This is a complex task requiring external libraries or Windows APIs for hardware detection.
            // Placeholder for future implementation:
            // 1. Detect connected input devices (steering wheels, lightguns, joysticks, gamepads).
            // 2. Read the game's TeknoParrot profile XML.
            // 3. Based on game type (e.g., driving, shooting, fighting) and detected hardware,
            //    modify the XML to map controls. This would involve specific XML nodes like
            //    <InputMapper> and <AxisConfig>.
            // 4. For lightguns, potentially include calibration settings.
        }

        public void ValidateGameProfile(string profilePath)
        {
            Console.WriteLine($"Validating TeknoParrot profile {profilePath}...");
            // Placeholder for future implementation:
            // 1. Load the XML profile.
            // 2. Validate against a known TeknoParrot XML schema (if available, or inferred).
            // 3. Check for valid file paths (GamePath, RomPath).
            // 4. Check for consistency in input mappings (if AutoConfigureInput has been run).
            // 5. Report errors or attempt to fix common issues.
        }
    }
}
