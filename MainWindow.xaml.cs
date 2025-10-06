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
using Arcadia.Launchers;
using Arcadia.Updater;

// Mandatory import for tab components
using Arcadia.UI.Tabs; 

namespace Arcadia.UI
{
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

            // Use GamesTab instead of GameWheelTab, and pass dependencies
            SwitchTab(new GamesTab(_games, _gameLauncher, _settingsManager));
        }

        private void InitializeServices()
        {
            // Use Path.Combine for safely constructing the AppData path
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Arcadia");
            
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
            LoadingOverlay.Visibility = Visibility.Visible;

            _games = await Task.Run(() =>
            {
                var games = _gameDatabase.GetAllGames();
                if (games.Count == 0)
                {
                    games = ScanForPCGames(games);
                }
                return games;
            });
            
            LoadingOverlay.Visibility = Visibility.Collapsed;

            if (_games.Count > 0)
            {
                UpdateGameDetails(_games[_selectedIndex]); 
            }
        }

        private List<Game> ScanForPCGames(List<Game> currentGames)
        {
            // Scanning logic remains here...
            return currentGames;
        }
        
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
        // NAVIGATION METHODS (All pass required dependencies)
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
            // FIX: Use GamesTab and pass dependency
            SwitchTab(new GamesTab(_games)); 
        }
        
        private void LibraryButton_Click(object sender, RoutedEventArgs e)
        {
            // Corrected: LibraryTab requires the List<Game> dependency
            SwitchTab(new LibraryTab(_games));
        }
        
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Corrected: SettingsTab requires the SettingsManager dependency
            SwitchTab(new SettingsTab(_settingsManager));
        }
        
        private void UpdaterButton_Click(object sender, RoutedEventArgs e)
        {
            // Corrected: UpdaterTab requires dependencies
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
