using System;
using System.Collections.Generic;
using SQLite;

namespace Arcadia.Core.Models
{
    public class Emulator
    {
        [PrimaryKey]
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ExecutablePath { get; set; } = string.Empty;
        public string CommandLineTemplate { get; set; } = string.Empty;
        public string SavePathTemplate { get; set; } = string.Empty;
        
        [Ignore]
        public Dictionary<string, string> CommandLineVariables { get; set; } = new();

        public string? Version { get; set; }
        public string SupportedPlatforms { get; set; } = string.Empty;
    }
}