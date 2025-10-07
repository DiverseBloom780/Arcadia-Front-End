using System;
using System.Collections.Generic;

namespace Arcadia.Core.Models
{
    /// <summary>
    /// Represents a game in the Arcadia library
    /// </summary>
    public class Game
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string Developer { get; set; } = string.Empty;
        public int? ReleaseYear { get; set; }
        public string Genre { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // File paths
        public string ExecutablePath { get; set; } = string.Empty;
        public string RomPath { get; set; } = string.Empty;
        
        // Media paths
        public string BoxArtPath { get; set; } = string.Empty;
        public string CartArtPath { get; set; } = string.Empty;
        public string LogoPath { get; set; } = string.Empty;
        public string FanArtPath { get; set; } = string.Empty;
        public string VideoPreviewPath { get; set; } = string.Empty;
        public string ThemePath { get; set; } = string.Empty;
        
        // Game metadata
        public int PlayerCount { get; set; } = 1;
        public bool IsMultiplayer => PlayerCount > 1;
        public bool IsFavorite { get; set; }
        public double PlayTime { get; set; } // in hours
        public DateTime? LastPlayed { get; set; }
        public int TimesPlayed { get; set; }
        public GameCompletionStatus CompletionStatus { get; set; }
        
        // Launch configuration
        public LaunchType LaunchType { get; set; }
        public string EmulatorId { get; set; } = string.Empty;
        public string LauncherId { get; set; } = string.Empty;
        public string CommandLineArgs { get; set; } = string.Empty;
        
        // Tags and collections
        public List<string> Tags { get; set; } = new List<string>();
        public List<string> Collections { get; set; } = new List<string>();
        
        // TeknoParrot specific
        public bool IsTeknoParrotGame { get; set; }
        public string TeknoParrotProfilePath { get; set; } = string.Empty;
        public TeknoParrotGameType TeknoParrotType { get; set; }
    }

    public enum LaunchType
    {
        Emulator,
        Steam,
        GOG,
        EpicGames,
        TeknoParrot,
        Standalone
    }

    public enum GameCompletionStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Mastered
    }

    public enum TeknoParrotGameType
    {
        None,
        Racing,
        Shooting,
        Fighting,
        Sports,
        Other
    }
}
