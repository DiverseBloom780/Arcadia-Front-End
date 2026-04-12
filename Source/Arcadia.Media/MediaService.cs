using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arcadia.Core.Models;
using Arcadia.Core.Data;
using Arcadia.Core.Services;

namespace Arcadia.Media
{
    public class MediaService
    {
        private readonly GameDatabase _db;
        private readonly string _baseMediaPath;

        public MediaService(GameDatabase db, string baseMediaPath)
        {
            _db = db;
            _baseMediaPath = baseMediaPath;
        }

        /// <summary>
        /// Validates existing media paths and identifies missing assets.
        /// </summary>
        public List<string> GetMissingAssetsReport()
        {
            var games = _db.GetAllGames();
            var missing = new List<string>();

            foreach (var game in games)
            {
                if (string.IsNullOrEmpty(game.BoxArtPath) || !File.Exists(game.BoxArtPath))
                    missing.Add($"{game.Title} (BoxArt)");
                
                if (string.IsNullOrEmpty(game.LogoPath) || !File.Exists(game.LogoPath))
                    missing.Add($"{game.Title} (Logo)");

                if (string.IsNullOrEmpty(game.VideoPreviewPath) || !File.Exists(game.VideoPreviewPath))
                    missing.Add($"{game.Title} (Video)");
            }

            return missing;
        }

        /// <summary>
        /// Attempts to resolve media paths automatically based on title and platform.
        /// </summary>
        public void AutoResolveLocalMedia(Game game)
        {
            string platformDir = Path.Combine(_baseMediaPath, game.Platform ?? "Unknown");
            if (!Directory.Exists(platformDir)) return;

            // Example: Look for 'Street Fighter.png' in Assets/Media/Arcade/Logos
            string potentialLogo = Path.Combine(platformDir, "Logos", $"{game.Title}.png");
            if (File.Exists(potentialLogo)) game.LogoPath = potentialLogo;

            string potentialBox = Path.Combine(platformDir, "BoxArt", $"{game.Title}.png");
            if (File.Exists(potentialBox)) game.BoxArtPath = potentialBox;

            _db.UpdateGame(game);
        }
    }
}