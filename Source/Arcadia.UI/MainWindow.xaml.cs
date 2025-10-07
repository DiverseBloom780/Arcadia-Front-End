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

// Mandatory import for tab components
using Arcadia.UI.Tabs; 

namespace Arcadia.UI
{
    public partial class MainWindow : Window
    {
        private SettingsManager _settingsManager = null!;
        private GitHubUpdater? _gitHubUpdater;
        private List<Game> _games = new List<Game>();
        private DispatcherTimer _clockTimer = null!;

        public MainWindow()
        {
            InitializeComponent();
            
            InitializeServices();
            InitializeClock();
            
            LoadGames(); 
            
            // Set initial view to GamesTab, passing dependency
            SwitchTab(new GamesTab(_games)); 
        }

        private void InitializeServices()
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Arcadia");
            Directory.CreateDirectory(appDataPath);
            
            string settingsPath = Path.Combine(appDataPath, "settings.json");
            
            _settingsManager = new SettingsManager(settingsPath);
            
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
                // TODO: Implement actual game loading from database
                _games = await Task.Run(() => 
                {
                    // Placeholder: Return empty list for now
                    // When GameDatabase is implemented, use:
                    // return _gameDatabase.GetAllGames();
                    return new List<Game>();
                });
                
                // Refresh the current tab if it's showing games
                if (ContentArea.Children.Count > 0 && ContentArea.Children[0] is GamesTab)
                {
                    SwitchTab(new GamesTab(_games));
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
        
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle keyboard shortcuts
            switch (e.Key)
            {
                case Key.F1:
                    SettingsButton_Click(sender, new RoutedEventArgs());
                    break;
                case Key.F2:
                    // TODO: Implement search functionality
                    break;
                case Key.Escape:
                    // Close application or return to previous screen
                    if (MessageBox.Show("Exit Arcadia?", "Confirm Exit", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Application.Current.Shutdown();
                    }
                    break;
            }
        }
    }
}