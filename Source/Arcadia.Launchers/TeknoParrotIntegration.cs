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
        private readonly string _teknoRoot;
        private readonly string _profilesPath;
        private readonly string _romsPath;

        public TeknoParrotIntegration(string teknoRoot)
        {
            _teknoRoot = teknoRoot;
            _profilesPath = Path.Combine(_teknoRoot, "GameProfiles");
            _romsPath = Path.Combine(_teknoRoot, "Roms");
        }

        public bool IsTeknoParrotInstalled()
        {
            return Directory.Exists(_teknoRoot) &&
                   Directory.Exists(_profilesPath) &&
                   Directory.Exists(_romsPath);
        }

        public List<Game> DetectInstalledGames()
        {
            var detectedGames = new List<Game>();

            if (!IsTeknoParrotInstalled())
                return detectedGames;

            foreach (string romFolder in Directory.GetDirectories(_romsPath))
            {
                string gameName = new DirectoryInfo(romFolder).Name;
                string profilePath = Path.Combine(_profilesPath, $"{gameName}.xml");

                if (!File.Exists(profilePath))
                {
                    GenerateBasicGameProfile(gameName, profilePath);
                }

                var game = ParseGameProfile(profilePath);
                if (game != null)
                {
                    detectedGames.Add(game);
                }
            }

            return detectedGames;
        }

        private void GenerateBasicGameProfile(string gameName, string profilePath)
        {
            // Try to find an executable inside the ROM folder
            string exeFile = Directory.GetFiles(Path.Combine(_romsPath, gameName), "*.exe").FirstOrDefault();

            if (exeFile == null)
            {
                Console.WriteLine($"No executable found for {gameName}, skipping profile generation.");
                return;
            }

            var doc = new XDocument(
                new XElement("GameProfile",
                    new XElement("GameName", gameName),
                    new XElement("GamePath", exeFile),
                    new XElement("Launcher", "TeknoParrot"),
                    new XElement("InputProfile", "Default")
                )
            );
            doc.Save(profilePath);
        }

        private Game? ParseGameProfile(string profilePath)
        {
            try
            {
                var doc = XDocument.Load(profilePath);
                var root = doc.Root;

                if (root == null || root.Name != "GameProfile") return null;

                string? gameName = root.Element("GameName")?.Value;
                string? gamePath = root.Element("GamePath")?.Value;
                string? launcher = root.Element("Launcher")?.Value;

                if (string.IsNullOrEmpty(gameName) || string.IsNullOrEmpty(gamePath) || launcher != "TeknoParrot")
                    return null;

                return new Game
                {
                    Title = gameName,
                    ExecutablePath = gamePath,   // ✅ use ExecutablePath instead of missing Path
                    Platform = "Arcade (TeknoParrot)"
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
            // TODO: Implement hardware detection + XML input mapping
        }

        public void ValidateGameProfile(string profilePath)
        {
            Console.WriteLine($"Validating TeknoParrot profile {profilePath}...");
            // TODO: Implement XML schema validation against TeknoParrot’s XSD
        }
    }
}
