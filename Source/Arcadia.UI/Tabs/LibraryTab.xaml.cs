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
            {
                GameList.Items.Add(game.Title);
            }
        }

        private void GameList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GameList.SelectedItem is string title)
            {
                // TODO: Hook into MainWindow’s UpdateGameDetails if desired
            }
        }
    }
}
