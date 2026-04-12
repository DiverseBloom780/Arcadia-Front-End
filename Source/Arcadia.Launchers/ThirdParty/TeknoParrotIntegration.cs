using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Arcadia.Core.Models;

namespace Arcadia.Launchers
{
    /// <summary>
    /// TeknoParrot integration with automatic profile generation and device configuration
    /// </summary>
    public class TeknoParrotIntegration
    {
        private string? _teknoParrotPath;
        private string? _gameProfilesPath;
        private string? _userProfilesPath;

        public TeknoParrotIntegration()
        {
            DetectTeknoParrotInstallation();
        }

        private void DetectTeknoParrotInstallation()
        {
            try
            {
                string[] commonPaths = new[]
                {
                    @"C:\TeknoParrot",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "TeknoParrot"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "TeknoParrot"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "TeknoParrot")
                };

                foreach (var path in commonPaths)
                {
                    if (Directory.Exists(path) && File.Exists(Path.Combine(path, "TeknoParrotUi.exe")))
                    {
                        _teknoParrotPath = path;
                        _gameProfilesPath = Path.Combine(path, "GameProfiles");
                        _userProfilesPath = Path.Combine(path, "UserProfiles");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting TeknoParrot installation: {ex.Message}");
            }
        }

        public List<Game> ScanRomsFolder(string romsFolderPath)
        {
            var games = new List<Game>();

            if (!Directory.Exists(romsFolderPath))
            {
                Console.WriteLine($"ROMs folder not found: {romsFolderPath}");
                return games;
            }

            try
            {
                // Scan for executable files and known game formats
                var executableFiles = Directory.GetFiles(romsFolderPath, "*.exe", SearchOption.AllDirectories);
                
                foreach (var exePath in executableFiles)
                {
                    try
                    {
                        var game = CreateGameFromExecutable(exePath);
                        if (game != null)
                        {
                            games.Add(game);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing executable {exePath}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning ROMs folder: {ex.Message}");
            }

            return games;
        }

        private Game? CreateGameFromExecutable(string executablePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(executablePath);
            string gameId = $"teknoparrot_{Guid.NewGuid()}";

            var game = new Game
            {
                Id = gameId,
                Title = fileName,
                Platform = "TeknoParrot",
                LaunchType = LaunchType.TeknoParrot,
                ExecutablePath = executablePath,
                IsTeknoParrotGame = true,
                TeknoParrotType = DetectGameType(fileName)
            };

            // Generate GameProfile XML
            string profilePath = GenerateGameProfile(game);
            if (!string.IsNullOrEmpty(profilePath))
            {
                game.TeknoParrotProfilePath = profilePath;
            }

            return game;
        }

        private TeknoParrotGameType DetectGameType(string gameName)
        {
            string lowerName = gameName.ToLowerInvariant();

            // Racing game keywords
            if (lowerName.Contains("racing") || lowerName.Contains("drift") || lowerName.Contains("rally") ||
                lowerName.Contains("outrun") || lowerName.Contains("daytona") || lowerName.Contains("sega race") ||
                lowerName.Contains("initial d") || lowerName.Contains("wangan") || lowerName.Contains("maximum tune"))
            {
                return TeknoParrotGameType.Racing;
            }

            // Shooting game keywords
            if (lowerName.Contains("shoot") || lowerName.Contains("gun") || lowerName.Contains("crisis") ||
                lowerName.Contains("dead") || lowerName.Contains("house") || lowerName.Contains("rambo") ||
                lowerName.Contains("operation ghost"))
            {
                return TeknoParrotGameType.Shooting;
            }

            // Fighting game keywords
            if (lowerName.Contains("fight") || lowerName.Contains("tekken") || lowerName.Contains("soul calibur") ||
                lowerName.Contains("virtua fighter") || lowerName.Contains("street fighter") ||
                lowerName.Contains("king of fighters") || lowerName.Contains("blazblue"))
            {
                return TeknoParrotGameType.Fighting;
            }

            // Sports game keywords
            if (lowerName.Contains("sport") || lowerName.Contains("tennis") || lowerName.Contains("golf") ||
                lowerName.Contains("baseball") || lowerName.Contains("soccer") || lowerName.Contains("football"))
            {
                return TeknoParrotGameType.Sports;
            }

            return TeknoParrotGameType.Other;
        }

        public string GenerateGameProfile(Game game)
        {
            if (string.IsNullOrEmpty(_gameProfilesPath))
            {
                Console.WriteLine("TeknoParrot GameProfiles path not set");
                return string.Empty;
            }

            try
            {
                Directory.CreateDirectory(_gameProfilesPath);

                string profileFileName = $"{game.Title.Replace(" ", "_")}.xml";
                string profilePath = Path.Combine(_gameProfilesPath, profileFileName);

                // Create GameProfile XML following TeknoParrot's schema
                var gameProfile = new XElement("GameProfile",
                    new XElement("GameName", game.Title),
                    new XElement("GamePath", game.ExecutablePath),
                    new XElement("TestMenuParameter", ""),
                    new XElement("TestMenuIsExecutable", "false"),
                    new XElement("ExtraParameters", ""),
                    new XElement("IconName", $"{game.Title}.png"),
                    new XElement("EmulationProfile", GetEmulationProfile(game.TeknoParrotType)),
                    new XElement("GameProfileRevision", "1"),
                    new XElement("HasSeparateTestMode", "false"),
                    new XElement("EmulatorType", "TeknoParrot"),
                    new XElement("Patreon", "false")
                );

                // Add input configuration based on game type
                var configValues = GenerateInputConfiguration(game.TeknoParrotType);
                gameProfile.Add(configValues);

                var doc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    gameProfile
                );

                doc.Save(profilePath);
                Console.WriteLine($"Generated GameProfile: {profilePath}");

                return profilePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating GameProfile: {ex.Message}");
                return string.Empty;
            }
        }

        private string GetEmulationProfile(TeknoParrotGameType gameType)
        {
            return gameType switch
            {
                TeknoParrotGameType.Racing => "LindberghWheel",
                TeknoParrotGameType.Shooting => "LindberghGun",
                TeknoParrotGameType.Fighting => "Lindbergh",
                TeknoParrotGameType.Sports => "Lindbergh",
                _ => "Lindbergh"
            };
        }

        private XElement GenerateInputConfiguration(TeknoParrotGameType gameType)
        {
            var configValues = new XElement("ConfigValues");

            switch (gameType)
            {
                case TeknoParrotGameType.Racing:
                    configValues.Add(
                        new XElement("FieldInformation",
                            new XElement("CategoryName", "General"),
                            new XElement("FieldName", "Input"),
                            new XElement("FieldValue", "Wheel"),
                            new XElement("FieldType", "Dropdown")
                        ),
                        new XElement("FieldInformation",
                            new XElement("CategoryName", "Wheel"),
                            new XElement("FieldName", "EnableForceWheel"),
                            new XElement("FieldValue", "1"),
                            new XElement("FieldType", "Bool")
                        )
                    );
                    break;

                case TeknoParrotGameType.Shooting:
                    configValues.Add(
                        new XElement("FieldInformation",
                            new XElement("CategoryName", "General"),
                            new XElement("FieldName", "Input"),
                            new XElement("FieldValue", "Gun"),
                            new XElement("FieldType", "Dropdown")
                        ),
                        new XElement("FieldInformation",
                            new XElement("CategoryName", "Gun"),
                            new XElement("FieldName", "EnableGun"),
                            new XElement("FieldValue", "1"),
                            new XElement("FieldType", "Bool")
                        )
                    );
                    break;

                case TeknoParrotGameType.Fighting:
                    configValues.Add(
                        new XElement("FieldInformation",
                            new XElement("CategoryName", "General"),
                            new XElement("FieldName", "Input"),
                            new XElement("FieldValue", "Joystick"),
                            new XElement("FieldType", "Dropdown")
                        )
                    );
                    break;

                default:
                    configValues.Add(
                        new XElement("FieldInformation",
                            new XElement("CategoryName", "General"),
                            new XElement("FieldName", "Input"),
                            new XElement("FieldValue", "Gamepad"),
                            new XElement("FieldType", "Dropdown")
                        )
                    );
                    break;
            }

            return configValues;
        }

        public bool ValidateGameProfile(string profilePath)
        {
            try
            {
                if (!File.Exists(profilePath))
                {
                    Console.WriteLine($"Profile file not found: {profilePath}");
                    return false;
                }

                var doc = XDocument.Load(profilePath);
                var root = doc.Root;

                if (root == null || root.Name != "GameProfile")
                {
                    Console.WriteLine("Invalid GameProfile XML: Missing root element");
                    return false;
                }

                // Validate required elements
                string[] requiredElements = { "GameName", "GamePath", "EmulationProfile" };
                foreach (var elementName in requiredElements)
                {
                    if (root.Element(elementName) == null)
                    {
                        Console.WriteLine($"Invalid GameProfile XML: Missing required element {elementName}");
                        return false;
                    }
                }

                // Validate GamePath exists
                string? gamePath = root.Element("GamePath")?.Value;
                if (string.IsNullOrEmpty(gamePath) || !File.Exists(gamePath))
                {
                    Console.WriteLine($"Invalid GameProfile XML: GamePath does not exist: {gamePath}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating GameProfile: {ex.Message}");
                return false;
            }
        }

        public bool IsTeknoParrotInstalled()
        {
            return !string.IsNullOrEmpty(_teknoParrotPath);
        }

        public string? GetTeknoParrotPath()
        {
            return _teknoParrotPath;
        }
    }
}
