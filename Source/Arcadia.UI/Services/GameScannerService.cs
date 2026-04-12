using System.Collections.Generic;
using Arcadia.Core.Data;
using Arcadia.Core.Models;
using Arcadia.Launchers;

namespace Arcadia.UI.Services
{
    public class GameScannerService
    {
        private readonly GameDatabase _db;
        private readonly SteamIntegration _steam;
        private readonly GOGIntegration _gog;
        private readonly EpicGamesIntegration _epic;

        public GameScannerService(GameDatabase db)
        {
            _db = db;
            _steam = new SteamIntegration();
            _gog = new GOGIntegration();
            _epic = new EpicGamesIntegration();
        }

        /// <summary>
        /// Scans all integrated PC launchers and updates the local database.
        /// </summary>
        public void ScanAllLaunchers()
        {
            var allGames = new List<Game>();
            
            allGames.AddRange(_steam.DetectInstalledGames());
            allGames.AddRange(_gog.DetectInstalledGames());
            allGames.AddRange(_epic.DetectInstalledGames());
            
            _db.AddOrUpdateGames(allGames);
        }
    }
}