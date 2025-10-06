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
            
            // Set initial view to GamesTab, passing dependency
            SwitchTab(new GamesTab(_games)); 
        }

        private void InitializeServices()
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Arcadia");
            Directory.CreateDirectory(appDataPath);
            
            string databasePath = Path.Combine(appDataPath, "games.db");
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
            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += (s, e) => ClockText.Text = DateTime.Now.ToString("HH:mm:ss");
            _clockTimer.Start();
            ClockText.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private async void LoadGames() { /* Loading logic here */ }
        private List<Game> ScanForPCGames(List<Game> currentGames) { /* Scanning logic here */ return currentGames; }
        private void RenderGameWheel() { /* Render logic here */ } 
        private void UpdateGameDetails(Game game) { /* Detail logic here */ }
        private void Window_KeyDown(object sender, KeyEventArgs e) { /* Key input logic here */ }
        private async Task CheckForApplicationUpdates() { /* Update check logic here */ }

        
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
            SwitchTab(new GamesTab(_games)); 
        }
        
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

    public enum WheelOrientation { Vertical, Horizontal, Curved }
}