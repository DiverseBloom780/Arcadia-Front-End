using System;
using System.IO;
using System.Xml.Linq;
using Arcadia.Core.Models;

namespace Arcadia.Launchers.TeknoParrot
{
    public class TeknoParrotService
    {
        /// <summary>
        /// Generates a TeknoParrot GameProfile XML based on Arcadia's Game model.
        /// This fulfills the "Critical Feature" requirement for automated setup.
        /// </summary>
        public void GenerateGameProfile(Game game, string outputPath)
        {
            if (string.IsNullOrEmpty(game.RomPath))
                throw new ArgumentException("ROM path is required for TeknoParrot profile generation.");

            var root = new XElement("GameProfile",
                new XElement("GameName", game.Title),
                new XElement("GamePath", game.RomPath),
                new XElement("TestMode", "false"),
                new XElement("Windowed", "false"),
                new XElement("UseBezel", "true"),
                new XElement("EmulationProfile", GetProfileNameFromType(game.TeknoParrotType)),
                new XElement("ConfigValues",
                    new XElement("Field", new XAttribute("Name", "FreePlay"), new XAttribute("Value", "True")),
                    new XElement("Field", new XAttribute("Name", "FullHD"), new XAttribute("Value", "True"))
                )
            );

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
            
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            
            doc.Save(outputPath);
        }

        private string GetProfileNameFromType(TeknoParrotGameType type)
        {
            return type switch
            {
                TeknoParrotGameType.Racing => "Lindbergh", // Example mappings
                TeknoParrotGameType.Shooting => "SegaRingEdge",
                _ => "Generic"
            };
        }

        public bool ValidateProfile(string profilePath)
        {
            if (!File.Exists(profilePath)) return false;
            try
            {
                var doc = XDocument.Load(profilePath);
                return doc.Element("GameProfile") != null;
            }
            catch
            {
                return false;
            }
        }
    }
}