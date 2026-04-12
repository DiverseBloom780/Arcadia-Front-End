using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Arcadia.Core.Models;
using Arcadia.Core.Services;

namespace Arcadia.UI.Tabs
{
    public partial class GamesTab : UserControl
    {
        private enum NavState { Systems, Games }
        private NavState _currentState = NavState.Systems;
        
        private DispatcherTimer? _attractTimer;
        private DateTime _lastInputTime = DateTime.Now;

        private readonly List<Platform> _platforms = new List<Platform>();
        private List<Game> _allGames = new List<Game>();
        private Platform? _selectedPlatform;
        private Game? _selectedGame;
        private readonly GameLauncher? _gameLauncher;

        public GamesTab(List<Game> games, GameLauncher? launcher = null)
        {
            InitializeComponent();
            _allGames = games;
            _gameLauncher = launcher;
            
            InitializePlatforms();
            
            Wheel.SetItems(_platforms);
            Wheel.SelectionChanged += Wheel_SelectionChanged;
            Wheel.PreviewKeyDown += Wheel_PreviewKeyDown;
            Wheel.MouseMove += (s, e) => ResetAttractTimer();
            
            if (_platforms.Count > 0)
            {
                UpdateMedia(_platforms[0]);
            }

            StartAttractTimer();
        }

        private void StartAttractTimer()
        {
            _attractTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _attractTimer.Tick += (s, e) => 
            {
                if ((DateTime.Now - _lastInputTime).TotalSeconds > 30)
                {
                    // Auto scroll
                    Wheel.InputHit(false); // Simulated down key
                }
            };
            _attractTimer.Start();
        }

        private void ResetAttractTimer()
        {
            _lastInputTime = DateTime.Now;
        }

        private void InitializePlatforms()
        {
            // Sample Platforms
            var arcade = new Platform { Title = "Arcade", Description = "Classic coin-op games" };
            arcade.Games = _allGames.Where(g => g.Platform == "Arcade").ToList();
            
            var pc = new Platform { Title = "PC Games", Description = "Modern PC titles" };
            pc.Games = _allGames.Where(g => g.Platform == "PC").ToList();

            _platforms.Add(arcade);
            _platforms.Add(pc);
        }
        
        public GamesTab() : this(new List<Game>())
        {
        }

        private void Wheel_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ResetAttractTimer();
            // Alphabetical Jump Visuals
            if (e.Key >= System.Windows.Input.Key.A && e.Key <= System.Windows.Input.Key.Z)
            {
                ShowJumpOverlay(e.Key.ToString());
                // Wheel handles the actual jump logic
                return;
            }

            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (_currentState == NavState.Systems && _selectedPlatform != null)
                {
                    EnterSystem(_selectedPlatform);
                    e.Handled = true;
                }
                else if (_currentState == NavState.Games && _selectedGame != null)
                {
                    LaunchGame(_selectedGame);
                    e.Handled = true;
                }
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                if (_currentState == NavState.Games)
                {
                    BackToSystems();
                    e.Handled = true;
                }
            }
            else if (e.Key == System.Windows.Input.Key.L) // Layout toggle for testing
            {
                ToggleWheelLayout();
            }
        }

        private DispatcherTimer? _jumpTimer;

        private void ShowJumpOverlay(string letter)
        {
            JumpLetter.Text = letter;
            JumpOverlay.Visibility = System.Windows.Visibility.Visible;

            if (_jumpTimer == null)
            {
                _jumpTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                _jumpTimer.Tick += (s, e) => { JumpOverlay.Visibility = System.Windows.Visibility.Collapsed; _jumpTimer.Stop(); };
            }
            _jumpTimer.Stop();
            _jumpTimer.Start();
        }

        private async void LaunchGame(Game game)
        {
            if (_gameLauncher == null) return;
            
            // Show launching overlay
            LaunchGameTitle.Text = game.Title;
            LaunchOverlay.Visibility = System.Windows.Visibility.Visible;
            Wheel.IsEnabled = false; // Disable input
            
            // Artificial delay for cinematic feel
            await System.Threading.Tasks.Task.Delay(1500);
            
            bool success = _gameLauncher.LaunchGame(game);
            
            if (!success)
            {
                MessageBox.Show($"Failed to launch {game.Title}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LaunchOverlay.Visibility = System.Windows.Visibility.Collapsed;
                Wheel.IsEnabled = true;
            }
            else
            {
                // In a real app, we might wait for the process to exit or minimize
                // For now, let's keep the overlay or hide it after a bit
                await System.Threading.Tasks.Task.Delay(3000);
                LaunchOverlay.Visibility = System.Windows.Visibility.Collapsed;
                Wheel.IsEnabled = true;
            }
        }

        private void ToggleWheelLayout()
        {
            if (Wheel.Orientation == Controls.WheelOrientation.Curved)
                Wheel.Orientation = Controls.WheelOrientation.Vertical;
            else if (Wheel.Orientation == Controls.WheelOrientation.Vertical)
                Wheel.Orientation = Controls.WheelOrientation.Horizontal;
            else
                Wheel.Orientation = Controls.WheelOrientation.Curved;
        }

        private async void EnterSystem(Platform platform)
        {
            Wheel.TargetZoom = 2.0; 
            await System.Threading.Tasks.Task.Delay(300);
            
            _currentState = NavState.Games;
            SystemTitle.Text = platform.Title;
            SystemTitle.Visibility = System.Windows.Visibility.Visible;
            
            Wheel.SetItems(platform.Games);
            Wheel.TargetZoom = 1.0;
        }

        private async void BackToSystems()
        {
            Wheel.TargetZoom = 0.5;
            await System.Threading.Tasks.Task.Delay(300);

            _currentState = NavState.Systems;
            SystemTitle.Visibility = System.Windows.Visibility.Collapsed;
            
            Wheel.SetItems(_platforms);
            Wheel.TargetZoom = 1.0;
        }

        private void Wheel_SelectionChanged(object? sender, object item)
        {
            if (item is Platform p) _selectedPlatform = p;
            if (item is Game g) _selectedGame = g;
            UpdateMedia(item);
        }

        private void UpdateMedia(object item)
        {
            if (item is Game game)
            {
                GameTitle.Text = game.Title;
                GameInfo.Text = $"{game.Platform} | {game.ReleaseYear} | {game.Developer}";
                
                UpdateLogo(game.LogoPath);
                UpdateVideo(game.VideoPreviewPath);
                UpdateBackground(game.FanArtPath);
            }
            else if (item is Platform platform)
            {
                GameTitle.Text = platform.Title;
                GameInfo.Text = platform.Description;
                
                UpdateLogo(platform.LogoPath);
                UpdateVideo(platform.VideoPath);
                UpdateBackground(platform.BackgroundPath);
            }
        }

        private void UpdateLogo(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                GameLogo.Source = new BitmapImage(new Uri(path));
                GameLogo.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                GameLogo.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void UpdateBackground(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                BackgroundImage.Source = new BitmapImage(new Uri(path));
                BackgroundImage.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                BackgroundImage.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void UpdateVideo(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                try
                {
                    BackgroundVideo.Source = new Uri(path);
                    BackgroundVideo.Play();
                    BackgroundVideo.Visibility = System.Windows.Visibility.Visible;
                }
                catch
                {
                    BackgroundVideo.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            else
            {
                BackgroundVideo.Stop();
                BackgroundVideo.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
    }
}
