using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Arcadia.Core.Models;
using Arcadia.Core.Services;
using Arcadia.UI.Controls;

namespace Arcadia.UI.Tabs
{
    public partial class GamesTab : UserControl
    {
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

        public GamesTab() : this(new List<Game>())
        {
        }

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
        }
    }
}
