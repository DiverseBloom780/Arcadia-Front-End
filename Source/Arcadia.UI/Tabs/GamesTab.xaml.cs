using System;
using System.Collections.Generic;
using System.IO;
<<<<<<< HEAD
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Arcadia.Core.Models;
=======
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Arcadia.Core.Models;
using Arcadia.Core.Services;
using Arcadia.UI.Controls;
>>>>>>> 282f32e (Overhaul Arcadia frontend and filter redistributables)

namespace Arcadia.UI.Tabs
{
    public partial class GamesTab : UserControl
    {
<<<<<<< HEAD
        private enum NavState { Systems, Games }
        private NavState _currentState = NavState.Systems;
        
        private DispatcherTimer? _attractTimer;
        private DateTime _lastInputTime = DateTime.Now;

        private readonly List<Platform> _platforms = new List<Platform>();
        private List<Game> _allGames = new List<Game>();
        private Platform? _selectedPlatform;

        public GamesTab(List<Game> games)
        {
            InitializeComponent();
            _allGames = games;
            
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
        
=======
        private List<Game> _games;
        private GameLauncher? _launcher;

        public GamesTab(List<Game> games, GameLauncher? launcher = null)
        {
            InitializeComponent();
            _games = games;
            _launcher = launcher;
            
            // Generate mock data if really empty (last resort)
            if (_games == null || _games.Count == 0)
            {
                _games = GenerateMockGames();
            }

            WheelControl.GameSelected += WheelControl_GameSelected;
        }

>>>>>>> 282f32e (Overhaul Arcadia frontend and filter redistributables)
        public GamesTab() : this(new List<Game>())
        {
        }

<<<<<<< HEAD
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
            }
            else if (item is Platform platform)
            {
                GameTitle.Text = platform.Title;
                GameInfo.Text = platform.Description;
                
                UpdateLogo(platform.LogoPath);
                UpdateVideo(platform.VideoPath);
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
=======
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
            WheelControl.SetWheelMode(WheelMode.Angled); // Hyperspin style
            WheelControl.LoadGames(_games);
        }

        private void WheelControl_GameSelected(object? sender, Game e)
        {
            // Update Text
            GameTitleText.Text = e.Title;
            GameDeveloperText.Text = $"{e.Developer ?? "Unknown Dev"} // {e.Publisher ?? "Unknown Pub"}";
            GameDescriptionText.Text = string.IsNullOrEmpty(e.Description) ? "No description available for this title." : e.Description;
            
            // Animate background transition for the "Premium" feel
            if (!string.IsNullOrEmpty(e.FanArtPath) && File.Exists(e.FanArtPath))
            {
                DoubleAnimation fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
                fadeOut.Completed += (s, ev) =>
                {
                    BackgroundImage.Source = new BitmapImage(new Uri(e.FanArtPath));
                    BackgroundImage.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0.4, TimeSpan.FromMilliseconds(400)));
                };
                BackgroundImage.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            }
        }

        public void NavigateWheel(bool up)
        {
            if (up) WheelControl.MovePrevious();
            else WheelControl.MoveNext();
        }

        public void TriggerPlay()
        {
            PlayButton_Click(this, new RoutedEventArgs());
        }

        public void SelectGame(int index)
        {
            if (index >= 0 && index < _games.Count)
                WheelControl.SelectIndex(index);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_games.Count == 0)
                return;

            var selectedGame = _games[WheelControl.GetSelectedIndex()];
            if (_launcher != null && selectedGame != null)
            {
                _launcher.LaunchGame(selectedGame);
            }
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.W)
            {
                WheelControl.MovePrevious();
                e.Handled = true;
            }
            else if (e.Key == Key.Down || e.Key == Key.S)
            {
                WheelControl.MoveNext();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                TriggerPlay();
                e.Handled = true;
            }
        }

        private List<Game> GenerateMockGames()
        {
            return new List<Game>
            {
                new Game { Title = "Cyberpunk Neo", Developer = "CD Studio", Publisher = "Project Red", Description = "An open-world, action-adventure story set in Night City, a megalopolis obsessed with power, glamour and body modification." },
                new Game { Title = "Halo: Infinite Loop", Developer = "343 Ind", Publisher = "Xbox Game", Description = "The legendary Halo series returns with the most expansive Master Chief campaign yet." },
                new Game { Title = "TeknoParrot Racer", Developer = "Sega Arcade", Publisher = "Sega", Description = "A high speed racing simulator natively supported by Arcadia's deep integration." },
                new Game { Title = "Elden Ring: Shadows", Developer = "FromSoft", Publisher = "Bandai", Description = "Rise, Tarnished, and be guided by grace to brandish the power of the Elden Ring and become an Elden Lord in the Lands Between." },
                new Game { Title = "God of War: Ragnarok", Developer = "Santa Monica", Publisher = "PlayStation", Description = "Embark on an epic and heartfelt journey as Kratos and Atreus struggle with holding on and letting go." },
                new Game { Title = "Persona 6", Developer = "Atlus", Publisher = "Sega", Description = "A brand new entry in the stylish JRPG franchise." },
                new Game { Title = "Street Fighter VI", Developer = "Capcom", Publisher = "Capcom", Description = "Here comes Capcom's newest challenger!" },
                new Game { Title = "Super Mario Odyssey 2", Developer = "Nintendo EPD", Publisher = "Nintendo", Description = "Join Mario on a massive, globe-trotting 3D adventure." },
                new Game { Title = "Forza Horizon 6", Developer = "Playground Games", Publisher = "Xbox", Description = "Explore vibrant open world landscapes with limitless, fun driving action." },
                new Game { Title = "Doom: Eternal Hell", Developer = "id Software", Publisher = "Bethesda", Description = "Hell's armies have invaded Earth. Become the Slayer." },
            };
>>>>>>> 282f32e (Overhaul Arcadia frontend and filter redistributables)
        }
    }
}
