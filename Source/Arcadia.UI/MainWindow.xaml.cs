using System;
using System.Collections.Generic;
using System.IO; 
using System.Linq;
using System.Threading.Tasks; 
using System.Windows;
using System.Windows.Controls; 
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Arcadia.Core.Models;
using Arcadia.Core.Services;
using Arcadia.Updater;
using Arcadia.Launchers;
using Arcadia.SmartWizard;
using Arcadia.UI.Services;
using SharpDX.XInput;

// Mandatory import for tab components
using Arcadia.UI.Tabs; 

namespace Arcadia.UI
{
    public partial class MainWindow : Window
    {
        private SettingsManager _settingsManager = null!;
        private GameDatabase _gameDatabase = null!;
        private GameLauncher _gameLauncher = null!;
        private GitHubUpdater? _gitHubUpdater;
        private GamepadService _gamepadService = null!;
        private SmartWizardService _smartWizardService = null!;
        private List<Game> _games = new List<Game>();
        private List<Game> _searchResults = new List<Game>();
        private DispatcherTimer _clockTimer = null!;

        public MainWindow()
        {
            InitializeComponent();
            
            InitializeServices();
            InitializeClock();
            
            LoadGames(); 
            
            // Set initial view to GamesTab, passing dependencies
            SwitchTab(new GamesTab(_games, _gameLauncher)); 
        }

        private void InitializeServices()
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Arcadia");
            Directory.CreateDirectory(appDataPath);
            
            string settingsPath = Path.Combine(appDataPath, "settings.json");
            string dbPath = Path.Combine(appDataPath, "games.db");
            
            _settingsManager = new SettingsManager(settingsPath);
            _gameDatabase = new GameDatabase(dbPath);
            _gameLauncher = new GameLauncher(_gameDatabase);
            _gamepadService = new GamepadService();
            _gamepadService.Start();
            _gamepadService.DPadPressed += OnGamepadDPadPressed;
            _gamepadService.ButtonPressed += OnGamepadButtonPressed;

            _smartWizardService = new SmartWizardService(_gameDatabase);

            // Initialize GitHubUpdater if update settings are configured
            if (_settingsManager.Settings?.UpdateSettings != null)
            {
                _gitHubUpdater = new GitHubUpdater(
                    _settingsManager.Settings.UpdateSettings.GitHubOwner,
                    _settingsManager.Settings.UpdateSettings.GitHubRepository,
                    _settingsManager.Settings.General.Version
                );
            }
        }

        private void InitializeClock()
        {
            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += (s, e) => ClockText.Text = DateTime.Now.ToString("HH:mm:ss");
            _clockTimer.Start();
            ClockText.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private async void LoadGames() 
        { 
            // Show loading overlay
            LoadingOverlay.Visibility = Visibility.Visible;
            
            try
            {
                _games = await Task.Run(() => 
                {
<<<<<<< HEAD
                    return new List<Game>
                    {
                        new Game { Title = "Aero Fighters", Platform = "Arcade", ReleaseYear = 1992, Developer = "Video System" },
                        new Game { Title = "After Burner II", Platform = "Arcade", ReleaseYear = 1987, Developer = "Sega" },
                        new Game { Title = "Altered Beast", Platform = "Arcade", ReleaseYear = 1988, Developer = "Sega" },
                        new Game { Title = "Bubble Bobble", Platform = "Arcade", ReleaseYear = 1986, Developer = "Taito" },
                        new Game { Title = "Cadillacs and Dinosaurs", Platform = "Arcade", ReleaseYear = 1993, Developer = "Capcom" },
                        new Game { Title = "Darkstalkers", Platform = "Arcade", ReleaseYear = 1994, Developer = "Capcom" },
                        new Game { Title = "Final Fight", Platform = "Arcade", ReleaseYear = 1989, Developer = "Capcom" },
                        new Game { Title = "Galaga", Platform = "Arcade", ReleaseYear = 1981, Developer = "Namco" },
                        new Game { Title = "Golden Axe", Platform = "Arcade", ReleaseYear = 1989, Developer = "Sega" },
                        new Game { Title = "Metal Slug", Platform = "Arcade", ReleaseYear = 1996, Developer = "Nazca" },
                        new Game { Title = "Out Run", Platform = "Arcade", ReleaseYear = 1986, Developer = "Sega" },
                        new Game { Title = "Pac-Man", Platform = "Arcade", ReleaseYear = 1980, Developer = "Namco" },
                        new Game { Title = "R-Type", Platform = "Arcade", ReleaseYear = 1987, Developer = "Irem" },
                        new Game { Title = "Street Fighter II", Platform = "Arcade", ReleaseYear = 1991, Developer = "Capcom" },
                        new Game { Title = "The King of Fighters '98", Platform = "Arcade", ReleaseYear = 1998, Developer = "SNK" },
                        new Game { Title = "Half-Life 2", Platform = "PC", ReleaseYear = 2004, Developer = "Valve" },
                        new Game { Title = "Portal 2", Platform = "PC", ReleaseYear = 2011, Developer = "Valve" },
                        new Game { Title = "Cyberpunk 2077", Platform = "PC", ReleaseYear = 2020, Developer = "CD Projekt RED" }
                    };
=======
                    var existingGames = _gameDatabase.GetAllGames();

                    // Remove runtime/tool entries imported by older scans.
                    var nonGameEntries = existingGames
                        .Where(game => LauncherUtils.IsNonGameApplication(game.Title, game.ExecutablePath))
                        .ToList();
                    foreach (var game in nonGameEntries)
                    {
                        _gameDatabase.DeleteGame(game.Id);
                        existingGames.Remove(game);
                    }
                    
                    // If no games, perform an initial scan
                    if (existingGames.Count == 0)
                    {
                        var allDetected = new List<Game>();
                        
                        var librarySettings = _settingsManager.Settings.GameLibrary;

                        if (librarySettings.ScanSteam)
                            allDetected.AddRange(new SteamIntegration().DetectInstalledGames());
                        if (librarySettings.ScanEpicGames)
                            allDetected.AddRange(new EpicGamesIntegration().DetectInstalledGames());
                        if (librarySettings.ScanGOG)
                            allDetected.AddRange(new GOGIntegration().DetectInstalledGames());
                        if (librarySettings.ScanTeknoParrot)
                            allDetected.AddRange(new TeknoParrotIntegration().DetectInstalledGames());

                        // Importers can overlap. Keep the first record for each stable identity.
                        allDetected = allDetected
                            .GroupBy(game => string.IsNullOrWhiteSpace(game.LauncherId)
                                ? $"{game.LaunchType}:{game.ExecutablePath}:{game.RomPath}"
                                : $"{game.LaunchType}:{game.LauncherId}", StringComparer.OrdinalIgnoreCase)
                            .Select(group => group.First())
                            .ToList();

                        foreach (var g in allDetected)
                        {
                            _gameDatabase.AddGame(g);
                        }
                        
                        return allDetected;
                    }
                    
                    return existingGames;
>>>>>>> 282f32e (Overhaul Arcadia frontend and filter redistributables)
                });
                
                // Refresh the current tab if it's showing games
                if (ContentArea.Children.Count > 0 && ContentArea.Children[0] is GamesTab)
                {
                    SwitchTab(new GamesTab(_games, _gameLauncher));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading games: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Hide loading overlay
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        // ====================================================================
        // NAVIGATION METHODS
        // ====================================================================

        private void SwitchTab(UserControl newTab)
        {
            if (ContentArea.Children.Count > 0)
            {
                ContentArea.Children.Clear();
            }
            ContentArea.Children.Add(newTab);
        }

        private void GamesButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchTab(new GamesTab(_games, _gameLauncher)); 
        }
        
        private void LibraryButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchTab(new LibraryTab(_games, _gameDatabase));
        }

        private void NewsButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchTab(new NewsTab());
        }
        
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchTab(new SettingsTab(_settingsManager));
        }
        
        private void UpdaterButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchTab(new UpdaterTab(_gitHubUpdater, _settingsManager));
        }

        private void SmartWizardButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchTab(new SmartWizardTab(_smartWizardService));
        }
        
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (SearchOverlay.Visibility == Visibility.Visible)
            {
                if (e.Key == Key.Escape)
                {
                    HideSearch();
                    e.Handled = true;
                }
                return;
            }

            // Handle keyboard shortcuts
            switch (e.Key)
            {
                case Key.F1:
                    SettingsButton_Click(sender, new RoutedEventArgs());
                    break;
                case Key.F2:
                    ShowSearch();
                    break;
                case Key.F3:
                    SwitchTab(new SmartWizardTab(_smartWizardService));
                    break;
                case Key.Escape:
                    // Close application or return to previous screen
                    if (MessageBox.Show("Exit Arcadia?", "Confirm Exit", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        System.Windows.Application.Current.Shutdown();
                    }
                    break;
            }

            e.Handled = true;
        }

        private void ShowSearch()
        {
            SearchOverlay.Visibility = Visibility.Visible;
            SearchBox.Clear();
            SearchResults.ItemsSource = _games;
            SearchBox.Focus();
        }

        private void HideSearch()
        {
            SearchOverlay.Visibility = Visibility.Collapsed;
            ContentArea.Focus();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = SearchBox.Text.Trim();
            if (query.Length == 0)
            {
                _searchResults = _games.ToList();
            }
            else
            {
                _searchResults = _games.Where(game =>
                    Contains(game.Title, query) ||
                    Contains(game.Platform, query) ||
                    Contains(game.Publisher, query) ||
                    Contains(game.Developer, query) ||
                    Contains(game.Genre, query) ||
                    game.Tags.Any(tag => Contains(tag, query))
                ).ToList();
            }

            SearchResults.ItemsSource = _searchResults;
            if (_searchResults.Count > 0)
                SearchResults.SelectedIndex = 0;
        }

        private static bool Contains(string? value, string query) =>
            !string.IsNullOrWhiteSpace(value) && value.Contains(query, StringComparison.OrdinalIgnoreCase);

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                SearchResults.Focus();
                if (SearchResults.SelectedIndex < SearchResults.Items.Count - 1)
                    SearchResults.SelectedIndex++;
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && SearchResults.SelectedItem is Game game)
            {
                LaunchSearchResult(game);
                e.Handled = true;
            }
        }

        private void SearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResults.SelectedItem is Game game)
                SearchResults.ScrollIntoView(game);
        }

        private void LaunchSearchResult(Game game)
        {
            HideSearch();
            var index = _games.FindIndex(candidate => candidate.Id == game.Id);
            if (index >= 0)
            {
                var gamesTab = new GamesTab(_games, _gameLauncher);
                SwitchTab(gamesTab);
                gamesTab.SelectGame(index);
                gamesTab.TriggerPlay();
            }
        }

        // ====================================================================
        // GAMEPAD INPUT HANDLING
        // ====================================================================

        private void OnGamepadDPadPressed(GamepadService.Direction dir)
        {
            Dispatcher.Invoke(() =>
            {
                if (ContentArea.Children.Count > 0 && ContentArea.Children[0] is GamesTab gamesTab)
                {
                    if (dir == GamepadService.Direction.Up) gamesTab.NavigateWheel(true);
                    else if (dir == GamepadService.Direction.Down) gamesTab.NavigateWheel(false);
                }

                // Tab switching with Left/Right
                if (dir == GamepadService.Direction.Left || dir == GamepadService.Direction.Right)
                {
                    NavigateTabs(dir == GamepadService.Direction.Right);
                }
            });
        }

        private void OnGamepadButtonPressed(GamepadButtonFlags button)
        {
            Dispatcher.Invoke(() =>
            {
                if (button.HasFlag(GamepadButtonFlags.A))
                {
                    if (ContentArea.Children.Count > 0 && ContentArea.Children[0] is GamesTab gamesTab)
                    {
                        gamesTab.TriggerPlay();
                    }
                }
                else if (button.HasFlag(GamepadButtonFlags.B))
                {
                    SwitchTab(new GamesTab(_games, _gameLauncher));
                }
                else if (button.HasFlag(GamepadButtonFlags.Start))
                {
                    SwitchTab(new SettingsTab(_settingsManager));
                }
            });
        }

        private void NavigateTabs(bool next)
        {
            // Simple tab rotation
            var currentTab = ContentArea.Children.Count > 0 ? ContentArea.Children[0] : null;
            if (currentTab is GamesTab) SwitchTab(next ? new LibraryTab(_games, _gameDatabase) : new UpdaterTab(_gitHubUpdater, _settingsManager));
            else if (currentTab is LibraryTab) SwitchTab(next ? new NewsTab() : new GamesTab(_games, _gameLauncher));
            else if (currentTab is NewsTab) SwitchTab(next ? new SettingsTab(_settingsManager) : new LibraryTab(_games, _gameDatabase));
            else if (currentTab is SettingsTab) SwitchTab(next ? new UpdaterTab(_gitHubUpdater, _settingsManager) : new LibraryTab(_games, _gameDatabase));
            else if (currentTab is UpdaterTab) SwitchTab(next ? new GamesTab(_games, _gameLauncher) : new SettingsTab(_settingsManager));
        }

        protected override void OnClosed(EventArgs e)
        {
            _gamepadService?.Dispose();
            base.OnClosed(e);
        }
    }
}
