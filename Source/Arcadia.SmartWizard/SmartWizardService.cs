using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Arcadia.Core.Models;
using Arcadia.Core.Services;

namespace Arcadia.SmartWizard
{
    public class SmartWizardService
    {
        private readonly GameDatabase _database;

        public SmartWizardService(GameDatabase database)
        {
            _database = database;
        }

        public async Task<bool> ApplyFix(WizardSuggestion suggestion)
        {
            var game = _database.GetGame(suggestion.GameId);
            if (game == null) return false;

            if (suggestion.Title.StartsWith("Missing Metadata"))
            {
                // We instantiate it via reflection or direct reference. 
                // We'll use direct reference but we need to add Arcadia.Media reference.
                var downloader = new Arcadia.Media.MediaDownloaderService();
                bool updated = await downloader.DownloadMetadataAsync(game);
                if (updated)
                {
                    _database.UpdateGame(game);
                }
                return updated;
            }
            else if (suggestion.Title.StartsWith("Broken Path"))
            {
                // Re-scan logic goes here (mock for now)
                return false;
            }

            return false;
        }

        public List<WizardSuggestion> GetSuggestions()
        {
            var suggestions = new List<WizardSuggestion>();
            var games = _database.GetAllGames();

            foreach (var game in games)
            {
                // Check if executable exists
                if (!string.IsNullOrEmpty(game.ExecutablePath) && !File.Exists(game.ExecutablePath) && !Directory.Exists(game.ExecutablePath))
                {
                    suggestions.Add(new WizardSuggestion
                    {
                        GameId = game.Id,
                        Title = $"Broken Path: {game.Title}",
                        Description = "The game executable could not be found at the specified location.",
                        Severity = SuggestionSeverity.High,
                        ActionLabel = "Re-scan Library"
                    });
                }

                // Check for missing metadata
                if (string.IsNullOrEmpty(game.BoxArtPath) || string.IsNullOrEmpty(game.Description))
                {
                    suggestions.Add(new WizardSuggestion
                    {
                        GameId = game.Id,
                        Title = $"Missing Metadata: {game.Title}",
                        Description = "This game is missing cover art or description.",
                        Severity = SuggestionSeverity.Medium,
                        ActionLabel = "Download Metadata"
                    });
                }
            }

            return suggestions;
        }
    }

    public class WizardSuggestion
    {
        public string GameId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public SuggestionSeverity Severity { get; set; }
        public string ActionLabel { get; set; } = "Fix";
    }

    public enum SuggestionSeverity
    {
        Low,
        Medium,
        High
    }
}
