// This file requires: Arcadia.Core project/assembly with Models.Game class
using System.Collections.Generic;
using System.Windows.Controls;
using Arcadia.Core.Models;

namespace Arcadia.UI.Tabs
{
    /// <summary>
    /// The LibraryTab provides a standard list view of all loaded games.
    /// </summary>
    public partial class LibraryTab : UserControl
    {
        private readonly List<Game> _games;

        /// <summary>
        /// Initializes a new instance of the LibraryTab UserControl.
        /// </summary>
        /// <param name="games">The list of games loaded from the database.</param>
        public LibraryTab(List<Game> games)
        {
            InitializeComponent();
            _games = games;

            // Populate the ListBox with game titles
            foreach (var game in _games)
                GameList.Items.Add(game.Title);
        }

        /// <summary>
        /// Handles the selection change event in the game list, displaying details for the selected game.
        /// </summary>
        private void GameList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check if the selected item is a string (the game title)
            if (GameList.SelectedItem is string title)
            {
                // Find the corresponding Game object in the list
                var selectedGame = _games.Find(g => g.Title == title);
                if (selectedGame != null)
                {
                    SelectedGameTitle.Text = selectedGame.Title;
                    
                    // Display details using string interpolation and null coalescing for safety
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