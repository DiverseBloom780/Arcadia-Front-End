using System.Collections.Generic;
using System.Windows.Controls;
using Arcadia.Core.Models;

namespace Arcadia.UI.Tabs
{
    public partial class LibraryTab : UserControl
    {
        private readonly List<Game> _games;

        public LibraryTab(List<Game> games)
        {
            InitializeComponent();
            _games = games;

            foreach (var game in _games)
                GameList.Items.Add(game.Title);
        }

        private void GameList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GameList.SelectedItem is string title)
            {
                var selectedGame = _games.Find(g => g.Title == title);
                if (selectedGame != null)
                {
                    SelectedGameTitle.Text = selectedGame.Title;
                    SelectedGameInfo.Text =
                        $"Genre: {selectedGame.Genre ?? "Unknown"}\n" +
                        $"Publisher: {selectedGame.Publisher ?? "Unknown"}\n" +
                        $"Year: {selectedGame.ReleaseYear?.ToString() ?? "Unknown"}\n" +
                        $"Platform: {selectedGame.Platform?.ToUpper() ?? "N/A"}";
                }
            }
        }
    }
}
