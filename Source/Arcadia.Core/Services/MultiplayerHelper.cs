using System.Collections.Generic;
using System.Linq;
using Arcadia.Core.Models;

namespace Arcadia.Core.Services
{
    public class MultiplayerHelper
    {
        /// <summary>
        /// Categorizes games into multiplayer-specific collections.
        /// </summary>
        public void TagMultiplayerGames(IEnumerable<Game> games)
        {
            foreach (var game in games)
            {
                // Logic can be expanded to parse metadata or use a known-db
                if (game.PlayerCount > 1)
                {
                    if (!game.Collections.Contains("Multiplayer"))
                        game.Collections.Add("Multiplayer");

                    if (game.PlayerCount >= 4 && !game.Collections.Contains("Party Games"))
                        game.Collections.Add("Party Games");
                }

                // Arcade specific logic (Fighting/Sports) usually implies 2-Player
                if (game.Platform == "Arcade" && 
                   (game.Genre?.Contains("Fighting") == true || game.Genre?.Contains("Sports") == true))
                {
                    if (!game.Collections.Contains("2-Player Head-to-Head"))
                        game.Collections.Add("2-Player Head-to-Head");
                }
            }
        }
    }
}