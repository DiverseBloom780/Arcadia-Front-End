using System;
using System.Collections.Generic;

namespace Arcadia.Core.Models
{
    /// <summary>
    /// Represents a gaming platform/system in Arcadia
    /// </summary>
    public class Platform
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string LogoPath { get; set; } = string.Empty;
        public string VideoPath { get; set; } = string.Empty;
        public string BackgroundPath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // Navigation
        public List<Game> Games { get; set; } = new List<Game>();
    }
}
