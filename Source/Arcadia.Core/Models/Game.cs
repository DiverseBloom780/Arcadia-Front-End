using System;
using System.Collections.Generic;

namespace Arcadia.Core.Models
{
    public class Game
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Platform { get; set; }
        public string? Publisher { get; set; }
        public string? Developer { get; set; }
        public int? ReleaseYear { get; set; }
        public string? Genre { get; set; }
        public string? Description { get; set; }
        public string? ExecutablePath { get; set; }
        public string? RomPath { get; set; }
        public string? BoxArtPath { get; set; }
        public string? CartArtPath { get; set; }
        public string? LogoPath { get; set; }
        public string? FanArtPath { get; set; }
        public string? VideoPreviewPath { get; set; }
        public string? ThemePath { get; set; }
        public int PlayerCount { get; set; } = 1;
        public bool IsFavorite { get; set; }
        public double PlayTime { get; set; }
        public DateTime? LastPlayed { get; set; }
        public int TimesPlayed { get; set; }
        public GameCompletionStatus CompletionStatus { get; set; }
        public LaunchType LaunchType { get; set; }
        public string? EmulatorId { get; set; }
        public string? LauncherId { get; set; }
        public string? CommandLineArgs { get; set; }
        public List<string> Tags { get; set; } = new();
        public List<string> Collections { get; set; } = new();
        
        // TeknoParrot Specifics
        public bool IsTeknoParrotGame { get; set; }
        public string? TeknoParrotProfilePath { get; set; }
        public TeknoParrotGameType TeknoParrotType { get; set; }
    }
}