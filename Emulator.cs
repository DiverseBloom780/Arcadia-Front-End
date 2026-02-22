using System.Collections.Generic;

namespace Arcadia.Core.Models
{
    /// <summary>
    /// Represents an emulator configuration in Arcadia
    /// </summary>
    public class Emulator
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ExecutablePath { get; set; } = string.Empty;
        public string WorkingDirectory { get; set; } = string.Empty;
        public List<string> SupportedPlatforms { get; set; } = new List<string>();
        public List<string> SupportedExtensions { get; set; } = new List<string>();
        
        // Command line templates
        public string CommandLineTemplate { get; set; } = string.Empty;
        public Dictionary<string, string> CommandLineVariables { get; set; } = new Dictionary<string, string>();
        
        // Configuration profiles
        public List<EmulatorProfile> Profiles { get; set; } = new List<EmulatorProfile>();
        public string ActiveProfileId { get; set; } = string.Empty;
        
        // Auto-download configuration
        public bool SupportsAutoDownload { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        
        // BIOS requirements
        public List<BiosRequirement> BiosRequirements { get; set; } = new List<BiosRequirement>();
    }

    public class EmulatorProfile
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();
        public string ConfigFilePath { get; set; } = string.Empty;
    }

    public class BiosRequirement
    {
        public string Name { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ExpectedPath { get; set; } = string.Empty;
        public string MD5Hash { get; set; } = string.Empty;
        public bool IsOptional { get; set; }
    }
}
