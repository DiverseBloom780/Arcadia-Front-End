using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Arcadia.Core.Models;
using Arcadia.Core.Services;
using Arcadia.Launchers;
using Arcadia.Updater;
using System.Threading.Tasks;

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
        }

        private void InitializeServices()
        {
            string databasePath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Arcadia",
                "games.db"
            );

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(databasePath)!);

            string settingsPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Arcadia",
                "settings.json"
            );
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(settingsPath)!);
            // Ensure default settings.json exists if not already present
            if (!System.IO.File.Exists(settingsPath))
            {
                System.IO.File.Copy("/home/ubuntu/Arcadia/Config/settings.json", settingsPath);
            }

            _settingsManager = new SettingsManager(settingsPath);
            _gameDatabase = new GameDatabase(databasePath);
            _gameLauncher = new GameLauncher(_gameDatabase);
            _gitHubUpdater = new GitHubUpdater(
                _settingsManager.Settings.UpdateSettings.GitHubOwner,
                _settingsManager.Settings.UpdateSettings.GitHubRepository,
                _settingsManager.Settings.General.Version
            );
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

        private void LoadGames()
        {
            LoadingOverlay.Visibility = Visibility.Visible;

            // Load games from database
            _games = _gameDatabase.GetAllGames();

            // If no games, scan for PC games
            if (_games.Count == 0)
            {
                ScanForPCGames();
            }

            GameCountText.Text = $"{_games.Count} Games";
            LoadingOverlay.Visibility = Visibility.Collapsed;

            if (_games.Count > 0)
            {
                UpdateGameDetails(_games[_selectedIndex]);
            }
        }

        private void ScanForPCGames()
        {
            // Scan Steam
            var steamIntegration = new SteamIntegration();
            if (steamIntegration.IsSteamInstalled())
            {
                var steamGames = steamIntegration.DetectInstalledGames();
                foreach (var game in steamGames)
                {
                    _gameDatabase.AddGame(game);
                    _games.Add(game);
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
                    _games.Add(game);
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
                    _games.Add(game);
                }
            }
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
                    MessageBox.Show($"Error launching game: {ex.Message}", "Launch Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenSettings()
        {
            // TODO: Implement settings window
            MessageBox.Show("Settings window coming soon!", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenSearch()
        {
            // TODO: Implement search window
            MessageBox.Show("Search window coming soon!", "Search", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void CheckForApplicationUpdates()
        {
            if (!_settingsManager.Settings.UpdateSettings.CheckForUpdatesOnStartup)
            {
                return;
            }

            // Assuming LoadingOverlay is a UI element defined in XAML
            // LoadingOverlay.Visibility = Visibility.Visible;
            // Optionally, update loading text to "Checking for updates..."

            try
            {
                var updateInfo = await _gitHubUpdater.CheckForUpdatesAsync();
                if (updateInfo != null)
                {
                    MessageBoxResult result = MessageBox.Show(
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
                            MessageBox.Show("Update downloaded and will be installed. Arcadia will restart.", "Update Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                            Application.Current.Shutdown();
                        }
                        else
                        {
                            MessageBox.Show("Failed to download and install update.", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    // MessageBox.Show("Arcadia is up to date.", "No Update", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking for updates: {ex.Message}", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }
    }

    public enum WheelOrientation
    {
        Vertical,
        Horizontal,
        Curved
    }
}
