using System;
using System.Collections.Generic;
using System.IO; 
using System.Linq;
using System.Threading.Tasks; 
using System.Windows;
using System.Windows.Controls; // Includes UserControl
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Arcadia.Core.Models;
using Arcadia.Core.Services;
using Arcadia.Launchers;
using Arcadia.Updater;

// Import the namespace where the new tabs will reside (assuming standard structure)
using Arcadia.UI.Tabs; 

namespace Arcadia.UI
{
    // The ContentArea Grid/Panel is defined in MainWindow.xaml
    public partial class MainWindow : Window
    {
        private GameDatabase _gameDatabase;
        private GameLauncher _gameLauncher;
        private SettingsManager _settingsManager;
        private GitHubUpdater _gitHubUpdater;
        private List<Game> _games = new List<Game>();
        private int _selectedIndex = 0;
        private DispatcherTimer _clockTimer;
        private WheelOrientation _wheelOrientation = WheelOrientation.Vertical;

        public MainWindow()
        {
            InitializeComponent();
            
            InitializeServices();
            InitializeClock();
            
            LoadGames(); 
            RenderGameWheel();
            CheckForApplicationUpdates();
            
            // Set the initial view to the Game Wheel when the window loads
            SwitchTab(new GameWheelTab(_games)); 
        }

        private void InitializeServices()
        {
            // Use Path.Combine for safely constructing the AppData path
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Arcadia");
            
            // Ensure the application data directory exists
            Directory.CreateDirectory(appDataPath);
            
            // --- Game Database Setup ---
            string databasePath = Path.Combine(appDataPath, "games.db");

            // --- Settings Setup ---
            string settingsPath = Path.Combine(appDataPath, "settings.json");
            
            _settingsManager = new SettingsManager(settingsPath);
            _gameDatabase = new GameDatabase(databasePath);
            _gameLauncher = new GameLauncher(_gameDatabase);
            
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
            _clockTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _clockTimer.Tick += (s, e) => ClockText.Text = DateTime.Now.ToString("HH:mm:ss");
            _clockTimer.Start();
            ClockText.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private async void LoadGames()
        {
            // Assuming LoadingOverlay is defined in XAML, removing the call since it wasn't provided previously
            // LoadingOverlay.Visibility = Visibility.Visible;

            _games = await Task.Run(() =>
            {
                var games = _gameDatabase.GetAllGames();

                if (games.Count == 0)
                {
                    games = ScanForPCGames(games);
                }
                return games;
            });

            // Assuming GameCountText is defined in XAML
            // GameCountText.Text = $"{_games.Count} Games";
            // LoadingOverlay.Visibility = Visibility.Collapsed;

            if (_games.Count > 0)
            {
                // Note: The logic for UpdateGameDetails/RenderGameWheel should ideally move to GameWheelTab
                // But for now, we leave the original calls which interact with controls assumed to be in the main window.
                UpdateGameDetails(_games[_selectedIndex]); 
            }
        }

        private List<Game> ScanForPCGames(List<Game> currentGames)
        {
            // Scan Steam
            var steamIntegration = new SteamIntegration();
            if (steamIntegration.IsSteamInstalled())
            {
                var steamGames = steamIntegration.DetectInstalledGames();
                foreach (var game in steamGames)
                {
                    _gameDatabase.AddGame(game);
                    currentGames.Add(game);
                }
            }

            // Scan GOG
            var gogIntegration = new GOGIntegration();
            if (gogIntegration.IsGOGGalaxyInstalled())
            {
                var gogGames = gogIntegration.DetectInstalledGames();
                foreach (var game in gogGames)
                {
                    _gameDatabase.AddGame(game);
                    currentGames.Add(game);
                }
            }

            // Scan Epic Games
            var epicIntegration = new EpicGamesIntegration();
            if (epicIntegration.IsEpicGamesInstalled())
            {
                var epicGames = epicIntegration.DetectInstalledGames();
                foreach (var game in epicGames)
                {
                    _gameDatabase.AddGame(game);
                    currentGames.Add(game);
                }
            }
            return currentGames;
        }
        
        // NOTE: This logic should ideally move to the GameWheelTab, 
        // but remains here for backward compatibility with unprovided controls.
        private void RenderGameWheel() { /* Existing Wheel Render Logic */ } 
        private void UpdateGameDetails(Game game) { /* Existing Detail Update Logic */ }
        private void Window_KeyDown(object sender, KeyEventArgs e) { /* Existing Key Down Logic */ }
        private void NavigateUp() { /* Existing Navigate Logic */ }
        private void NavigateDown() { /* Existing Navigate Logic */ }
        private void LaunchSelectedGame() { /* Existing Launch Logic */ }
        private void OpenSettings() { /* Existing Settings Logic */ }
        private void OpenSearch() { /* Existing Search Logic */ }
        private async Task CheckForApplicationUpdates() { /* Existing Update Check Logic */ }

        
        // ====================================================================
        // NAVIGATION METHODS
        // ====================================================================

        /// <summary>
        /// Clears the ContentArea container and adds a new UserControl as the active view.
        /// NOTE: 'ContentArea' is defined as a Grid in MainWindow.xaml.
        /// </summary>
        private void SwitchTab(UserControl newTab)
        {
            if (ContentArea.Children.Count > 0)
            {
                ContentArea.Children.Clear();
            }
            ContentArea.Children.Add(newTab);
        }

        /// <summary>
        /// Handler for the Games button to show the main Game Wheel.
        /// </summary>
        private void GamesButton_Click(object sender, RoutedEventArgs e)
        {
            // Use the new GameWheelTab
            SwitchTab(new GameWheelTab(_games)); 
        }
        
        // Handlers for the new sidebar buttons
        
        private void LibraryButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchTab(new LibraryTab(_games)); 
        }
        
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchTab(new SettingsTab(_settingsManager));
        }
        
        private void UpdaterButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchTab(new UpdaterTab(_gitHubUpdater, _settingsManager));
        }
    }

    public enum WheelOrientation
    {
        Vertical,
        Horizontal,
        Curved
    }
}
