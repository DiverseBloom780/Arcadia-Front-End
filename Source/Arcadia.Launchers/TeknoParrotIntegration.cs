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
        private string? _teknoParrotPath;
        private string? _userProfilesPath;

        public TeknoParrotIntegration()
        {
            DetectTeknoParrot();
        }

        private void DetectTeknoParrot()
        {
            try
            {
                // Typical paths
                string[] searchPaths = new[]
                {
                    @"C:\TeknoParrot",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "TeknoParrot"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "TeknoParrot")
                };

                foreach (var path in searchPaths)
                {
                    if (Directory.Exists(path))
                    {
                        _teknoParrotPath = path;
                        _userProfilesPath = Path.Combine(path, "UserProfiles");
                        break;
                    }
                }
            }
            catch { }
        }

        public List<Game> DetectInstalledGames()
        {
            var games = new List<Game>();
            if (string.IsNullOrEmpty(_userProfilesPath) || !Directory.Exists(_userProfilesPath))
                return games;

            try
            {
                var xmlFiles = Directory.GetFiles(_userProfilesPath, "*.xml");
                foreach (var xmlFile in xmlFiles)
                {
                    var game = ParseTeknoProfile(xmlFile);
                    if (game != null)
                    {
                        games.Add(game);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning TeknoParrot profiles: {ex.Message}");
            }

            return games;
        }

        private Game? ParseTeknoProfile(string xmlPath)
        {
            try
            {
                var doc = XDocument.Load(xmlPath);
                var root = doc.Root;
                if (root == null) return null;

                string? gameName = root.Element("GameName")?.Value;
                if (string.IsNullOrEmpty(gameName)) return null;

                return new Game
                {
                    Id = $"tp_{Path.GetFileNameWithoutExtension(xmlPath)}",
                    Title = gameName,
                    Platform = "Arcade",
                    LaunchType = LaunchType.TeknoParrot,
                    IsTeknoParrotGame = true,
                    TeknoParrotProfilePath = xmlPath,
                    TeknoParrotType = DetectGameType(gameName)
                };
            }
            catch
            {
                return null;
            }
        }

        private TeknoParrotGameType DetectGameType(string title)
        {
            string t = title.ToLower();
            if (t.Contains("racing") || t.Contains("drift") || t.Contains("wangan") || t.Contains("initial d"))
                return TeknoParrotGameType.Racing;
            if (t.Contains("shooting") || t.Contains("gun") || t.Contains("house of the dead") || t.Contains("time crisis"))
                return TeknoParrotGameType.Shooting;
            if (t.Contains("fight") || t.Contains("street fighter") || t.Contains("tekken"))
                return TeknoParrotGameType.Fighting;
            
            return TeknoParrotGameType.Other;
        }
    }
}
