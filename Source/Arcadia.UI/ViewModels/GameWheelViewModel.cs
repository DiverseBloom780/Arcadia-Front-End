using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Arcadia.Core.Models;
using Arcadia.Core.Services;
using Arcadia.Core.Data;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;

namespace Arcadia.UI.ViewModels
{
    public class GameWheelViewModel : INotifyPropertyChanged
    {
        private readonly GameDatabase _db;
        private readonly GameLauncher _launcher;
        private double _currentScrollOffset; // For smooth interpolation
        private double _targetScrollOffset;
        private int _selectedIndex;
        private DateTime _lastTick = DateTime.Now;
        private const double SpeedMultiplier = 10.0; 

        private WindowState _currentWindowState = WindowState.Normal;
        public WindowState CurrentWindowState
        {
            get => _currentWindowState;
            set { _currentWindowState = value; OnPropertyChanged(nameof(CurrentWindowState)); }
        }

        public ObservableCollection<Game> Games { get; } = new();

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    _targetScrollOffset = value;
                    OnPropertyChanged(nameof(SelectedIndex));
                    OnPropertyChanged(nameof(SelectedGame));
                }
            }
        }

        public Game? SelectedGame => Games.Count > 0 ? Games[SelectedIndex] : null;

        public double CurrentScrollOffset
        {
            get => _currentScrollOffset;
            set
            {
                _currentScrollOffset = value;
                OnPropertyChanged(nameof(CurrentScrollOffset));
            }
        }

        public GameWheelViewModel(GameDatabase db, GameLauncher launcher)
        {
            _db = db;
            _launcher = launcher;
            LoadGames();
            
            // Hook into the CompositionTarget.Rendering event for smooth per-frame updates
            CompositionTarget.Rendering += OnRenderingTick;
        }

        public void HandleKeyDown(Key key)
        {
            switch (key)
            {
                case Key.Up:
                case Key.W:
                    MovePrevious();
                    break;
                case Key.Down:
                case Key.S:
                    MoveNext();
                    break;
                case Key.Enter:
                case Key.Space:
                    LaunchSelected();
                    break;
                case Key.F11:
                    CurrentWindowState = CurrentWindowState == WindowState.Maximized 
                        ? WindowState.Normal 
                        : WindowState.Maximized;
                    break;
            }
        }

        private async void LaunchSelected()
        {
            if (SelectedGame != null)
            {
                await _launcher.LaunchGameAsync(SelectedGame);
            }
        }

        private void OnRenderingTick(object? sender, EventArgs e)
        {
            var now = DateTime.Now;
            double deltaTime = (now - _lastTick).TotalSeconds;
            _lastTick = now;

            if (Math.Abs(CurrentScrollOffset - _targetScrollOffset) > 0.001)
            {
                // Frame-rate independent smoothing
                double step = (_targetScrollOffset - CurrentScrollOffset) * SpeedMultiplier * deltaTime;
                CurrentScrollOffset += step;
            }
            else
            {
                CurrentScrollOffset = _targetScrollOffset;
            }
        }

        public void LoadGames()
        {
            var games = _db.GetGames();
            Games.Clear();
            foreach (var game in games) Games.Add(game);
        }

        public void MoveNext()
        {
            if (Games.Count == 0) return;
            _targetScrollOffset++;
            _selectedIndex = (int)Math.Round(_targetScrollOffset) % Games.Count;
        }

        public void MovePrevious()
        {
            if (Games.Count == 0) return;
            _targetScrollOffset--;
            _selectedIndex = (int)((Math.Round(_targetScrollOffset) % Games.Count) + Games.Count) % Games.Count;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}