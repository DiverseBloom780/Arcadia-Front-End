// This file requires: Arcadia.Core project/assembly with Models.Game class
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using Arcadia.Core.Models;
using Arcadia.Core.Services;

namespace Arcadia.UI.Tabs
{
    /// <summary>
    /// The LibraryTab provides a standard list view of all loaded games.
    /// </summary>
    public partial class LibraryTab : UserControl
    {
        private readonly List<Game> _games;
        private readonly GameDatabase? _gameDatabase;

        /// <summary>
        /// Initializes a new instance of the LibraryTab UserControl.
        /// </summary>
        /// <param name="games">The list of games loaded from the database.</param>
        public LibraryTab(List<Game> games, GameDatabase? gameDatabase = null)
        {
            InitializeComponent();
            _games = games;
            _gameDatabase = gameDatabase;

            // Set the ItemsSource directly since we use data binding in XAML
            GameListView.ItemsSource = _games;
        }

        private void RefreshList()
        {
            GameListView.Items.Refresh();
        }

        /// <summary>
        /// Handles the selection change event in the game list, displaying details for the selected game.
        /// </summary>
        private void GameListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // The selected item is a Game object because we bound ItemsSource to List<Game>
            if (GameListView.SelectedItem is Game selectedGame)
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
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = SearchBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                GameListView.ItemsSource = _games;
            }
            else
            {
                GameListView.ItemsSource = _games.Where(game =>
                    Contains(game.Title, query) ||
                    Contains(game.Platform, query) ||
                    Contains(game.Publisher, query) ||
                    Contains(game.Developer, query) ||
                    Contains(game.Genre, query) ||
                    game.Tags.Any(tag => Contains(tag, query))).ToList();
            }
        }

        private static bool Contains(string? value, string query) =>
            !string.IsNullOrWhiteSpace(value) && value.Contains(query, System.StringComparison.OrdinalIgnoreCase);

        private void SearchBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            SearchPlaceholder.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void SearchBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchBox.Text))
                SearchPlaceholder.Visibility = System.Windows.Visibility.Visible;
        }

        private void AddManualGame_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Games|*.exe;*.elf;*.pkg|PC Executables (*.exe)|*.exe|PS4 Games (*.elf;*.pkg)|*.elf;*.pkg|All files (*.*)|*.*",
                Title = "Select Game Executable"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var fileName = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                var extension = System.IO.Path.GetExtension(openFileDialog.FileName).ToLower();
                
                var isPS4 = extension == ".elf" || extension == ".pkg";
                
                // For emulators, ExecutablePath is the emulator, RomPath is the game file
                string exePath = isPS4 ? @"C:\Program Files\LayraPS4\LayraPS4.exe" : openFileDialog.FileName;
                string romPath = isPS4 ? openFileDialog.FileName : string.Empty;

                var newGame = new Game
                {
                    Id = "manual_" + System.Guid.NewGuid().ToString("N"),
                    Title = fileName,
                    ExecutablePath = exePath,
                    RomPath = romPath,
                    Platform = isPS4 ? "PS4" : "PC",
                    LaunchType = isPS4 ? LaunchType.Emulator : LaunchType.Standalone,
                    EmulatorId = isPS4 ? "LayraPS4" : ""
                };

                _games.Insert(0, newGame);
                RefreshList();
                GameListView.SelectedItem = newGame;
                
                System.Windows.MessageBox.Show($"'{fileName}' has been added to your library.", "Game Added", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        private void EditMetadata_Click(object sender, RoutedEventArgs e)
        {
            if (GameListView.SelectedItem is not Game selectedGame)
            {
                MessageBox.Show("Select a game first.", "Edit Metadata", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new Window
            {
                Title = "Rename Game",
                Width = 460,
                Height = 170,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                ResizeMode = ResizeMode.NoResize,
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(17, 17, 17))
            };

            var nameBox = new TextBox { Text = selectedGame.Title, Margin = new Thickness(0, 0, 0, 14), FontSize = 16 };
            var saveButton = new Button { Content = "SAVE", Width = 100, IsDefault = true, HorizontalAlignment = HorizontalAlignment.Right };
            saveButton.Click += (_, _) =>
            {
                string newTitle = nameBox.Text.Trim();
                if (newTitle.Length == 0) return;
                selectedGame.Title = newTitle;
                _gameDatabase?.UpdateGame(selectedGame);
                GameListView.Items.Refresh();
                SelectedGameTitle.Text = newTitle;
                dialog.DialogResult = true;
                dialog.Close();
            };

            dialog.Content = new Border
            {
                Padding = new Thickness(18),
                Child = new StackPanel
                {
                    Children = { new TextBlock { Text = "Display name", Foreground = System.Windows.Media.Brushes.White, Margin = new Thickness(0, 0, 0, 6) }, nameBox, saveButton }
                }
            };
            dialog.ShowDialog();
        }
    }
}
