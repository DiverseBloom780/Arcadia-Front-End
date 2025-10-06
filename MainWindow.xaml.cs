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
using Arcadia.UI.Tabs;
using System.Windows.Controls;
using System.Windows.Media.Animation;


namespace Arcadia.UI
{
    // The ContentArea Grid/Panel is assumed to be defined in MainWindow.xaml
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
            
            // NOTE: The hardcoded path issue has been corrected by assuming embedded resources 
            // or removal of the failing File.Copy line.
            
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

            GameCountText.Text = $"{_games.Count} Games";
            LoadingOverlay.Visibility = Visibility.Collapsed;

            if (_games.Count > 0)
            {
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

        private void RenderGameWheel()
        {
            GameWheelCanvas.Children.Clear();

            if (_games.Count == 0)
                return;

            int visibleItems = 7;
            double centerY = GameWheelCanvas.Height / 2;
            double centerX = GameWheelCanvas.Width / 2;
            double itemHeight = 100;
            double itemSpacing = 120;

            for (int i = -visibleItems / 2; i <= visibleItems / 2; i++)
            {
                int index = (_selectedIndex + i + _games.Count) % _games.Count;
                if (index < 0) index += _games.Count;

                var game = _games[index];
                double yPosition = centerY + (i * itemSpacing);
                double scale = 1.0 - Math.Abs(i) * 0.15;
                double opacity = 1.0 - Math.Abs(i) * 0.2;

                if (opacity < 0.2) opacity = 0.2;
                if (scale < 0.5) scale = 0.5;

                // Create game item
                var border = new System.Windows.Controls.Border
                {
                    Width = 500,
                    Height = itemHeight,
                    Background = i == 0 ? new SolidColorBrush(Color.FromArgb(80, 0, 217, 255)) : new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                    CornerRadius = new CornerRadius(10),
                    BorderBrush = i == 0 ? new SolidColorBrush(Color.FromRgb(0, 217, 255)) : Brushes.Transparent,
                    BorderThickness = new Thickness(i == 0 ? 3 : 0)
                };

                var textBlock = new System.Windows.Controls.TextBlock
                {
                    Text = game.Title,
                    FontSize = 24,
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontWeight = i == 0 ? FontWeights.Bold : FontWeights.Normal
                };

                border.Child = textBlock;

                var transform = new TransformGroup();
                transform.Children.Add(new ScaleTransform(scale, scale));
                transform.Children.Add(new TranslateTransform(centerX - 250, yPosition - itemHeight / 2));

                border.RenderTransform = transform;
                border.Opacity = opacity;

                GameWheelCanvas.Children.Add(border);
            }
        }

        private void UpdateGameDetails(Game game)
        {
            GameTitle.Text = game.Title;
            GameYear.Text = game.ReleaseYear?.ToString() ?? "Unknown";
            GameGenre.Text = game.Genre ?? "Unknown";
            GamePublisher.Text = game.Publisher ?? "Unknown";
            GameDescription.Text = game.Description ?? "No description available.";
            PlayTimeText.Text = $"{game.PlayTime:F1}h";
            TimesPlayedText.Text = game.TimesPlayed.ToString();
            PlayerCountText.Text = game.PlayerCount.ToString();
            PlatformText.Text = game.Platform.ToUpper();

            // Load game logo if available
            if (!string.IsNullOrEmpty(game.LogoPath) && System.IO.File.Exists(game.LogoPath))
            {
                GameLogo.Source = new BitmapImage(new Uri(game.LogoPath));
                GameLogo.Visibility = Visibility.Visible;
            }
            else
            {
                GameLogo.Visibility = Visibility.Collapsed;
            }

            // Load background video if available
            if (!string.IsNullOrEmpty(game.VideoPreviewPath) && System.IO.File.Exists(game.VideoPreviewPath))
            {
                BackgroundVideo.Source = new Uri(game.VideoPreviewPath);
                BackgroundVideo.Play();
            }
            else
            {
                BackgroundVideo.Stop();
                BackgroundVideo.Source = null;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (_games.Count == 0)
                return;

            switch (e.Key)
            {
                case Key.Up:
                case Key.W:
                    NavigateUp();
                    break;

                case Key.Down:
                case Key.S:
                    NavigateDown();
                    break;

                case Key.Enter:
                case Key.Space:
                    LaunchSelectedGame();
                    break;

                case Key.F1:
                    OpenSettings();
                    break;

                case Key.F2:
                    OpenSearch();
                    break;

                case Key.Escape:
                    Close();
                    break;
            }
        }

        private void NavigateUp()
        {
            _selectedIndex--;
            if (_selectedIndex < 0)
                _selectedIndex = _games.Count - 1;

            RenderGameWheel();
            UpdateGameDetails(_games[_selectedIndex]);
        }

        private void NavigateDown()
        {
            _selectedIndex++;
            if (_selectedIndex >= _games.Count)
                _selectedIndex = 0;

            RenderGameWheel();
            UpdateGameDetails(_games[_selectedIndex]);
        }

        private void LaunchSelectedGame()
        {
            if (_selectedIndex >= 0 && _selectedIndex < _games.Count)
            {
                var game = _games[_selectedIndex];
                
                try
                {
                    _gameLauncher.LaunchGame(game);
                    
                    // Optionally minimize or hide the window
                    // WindowState = WindowState.Minimized;
                }
                catch (Exception ex)
                {
                    // FIX 5: Using WPF System.Windows.MessageBox
                    System.Windows.MessageBox.Show(
                        $"Error launching game: {ex.Message}", 
                        "Launch Error", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error
                    );
                }
            }
        }

        private void OpenSettings()
        {
            // TODO: Implement settings window
            System.Windows.MessageBox.Show("Settings window coming soon!", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenSearch()
        {
            // TODO: Implement search window
            System.Windows.MessageBox.Show("Search window coming soon!", "Search", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        // FIX 6: Moved method inside the class and changed to async Task for proper asynchronous operation
        private async Task CheckForApplicationUpdates()
        {
            if (_settingsManager.Settings.UpdateSettings.CheckForUpdatesOnStartup == false)
            {
                return;
            }

            LoadingOverlay.Visibility = Visibility.Visible;
            // Optionally, update loading text to "Checking for updates..."

            try
            {
                // Assuming CheckForUpdatesAsync is already using HttpClient (or similar) and is non-blocking
                var updateInfo = await _gitHubUpdater.CheckForUpdatesAsync(); 
                
                if (updateInfo != null)
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show(
                        $"A new version of Arcadia ({updateInfo.Version}) is available!\n\nRelease Notes:\n{updateInfo.ReleaseNotes}\n\nDo you want to download and install it now?",
                        "Arcadia Update Available",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        // Implement progress reporting if needed
                        bool success = await _gitHubUpdater.DownloadAndInstallUpdateAsync(updateInfo);
                        if (success)
                        {
                            System.Windows.MessageBox.Show("Update downloaded and will be installed. Arcadia will restart.", "Update Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                            Application.Current.Shutdown();
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Failed to download and install update.", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error checking for updates: {ex.Message}", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }
        
        // ====================================================================
        // NAVIGATION METHODS
        // ====================================================================

        /// <summary>
        /// Clears the ContentArea container and adds a new UserControl as the active view.
        /// NOTE: 'ContentArea' must be defined as a Grid, StackPanel, or similar Panel in MainWindow.xaml.
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
        /// Example handler for a button click to switch to the Games Tab view.
        /// NOTE: This assumes a UserControl named 'GamesTab' exists in your project.
        /// </summary>
        private void GamesButton_Click(object sender, RoutedEventArgs e)
        {
            // You should replace the MessageBox below with the actual tab switch:
            // SwitchTab(new GamesTab()); 
            
            // Temporary placeholder since the GamesTab class is not provided:
            System.Windows.MessageBox.Show("Games Tab Switch Requested!", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public enum WheelOrientation
    {
        Vertical,
        Horizontal,
        Curved
    }
}
