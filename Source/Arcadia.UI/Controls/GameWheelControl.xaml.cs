using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Arcadia.Core;
using Arcadia.Core.Models;

namespace Arcadia.UI.Controls
{
    public partial class GameWheelControl : UserControl
    {
        private List<Game> _games = new List<Game>();
        private List<Border> _gameUIElements = new List<Border>();
        
        private int _selectedIndex = 0;
        private WheelMode _currentMode = WheelMode.Angled; // Hyperspin style by default
        private double _animationDurationMs = 250.0;

        public event EventHandler<Game>? GameSelected;

        public GameWheelControl()
        {
            InitializeComponent();
            this.SizeChanged += GameWheelControl_SizeChanged;
        }

        public void LoadGames(List<Game> games)
        {
            _games = games ?? new List<Game>();
            _selectedIndex = 0;
            BuildUIElements();
            UpdateLayoutPositions(false);
            
            if (_games.Count > 0)
            {
                GameSelected?.Invoke(this, _games[_selectedIndex]);
            }
        }

        public void SetWheelMode(WheelMode mode)
        {
            _currentMode = mode;
            UpdateLayoutPositions(true);
        }

        // Exposing navigation so GamesTab can tie keys to these
        public void MoveNext()
        {
            if (_games.Count == 0) return;
            _selectedIndex = (_selectedIndex + 1) % _games.Count;
            UpdateLayoutPositions(true);
            GameSelected?.Invoke(this, _games[_selectedIndex]);
        }

        public void MovePrevious()
        {
            if (_games.Count == 0) return;
            _selectedIndex = (_selectedIndex - 1 + _games.Count) % _games.Count;
            UpdateLayoutPositions(true);
            GameSelected?.Invoke(this, _games[_selectedIndex]);
        }

        public int GetSelectedIndex() => _selectedIndex;

        public void SelectIndex(int index)
        {
            if (index < 0 || index >= _games.Count)
                return;

            _selectedIndex = index;
            UpdateLayoutPositions(true);
            GameSelected?.Invoke(this, _games[_selectedIndex]);
        }

        private void GameWheelControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateLayoutPositions(false);
        }

        private void BuildUIElements()
        {
            WheelCanvas.Children.Clear();
            _gameUIElements.Clear();

            for (var i = 0; i < _games.Count; i++)
            {
                var game = _games[i];
                // Create a vibrant card for the game item
                Border card = new Border
                {
                    Width = 300,
                    Height = 100,
                    Background = new SolidColorBrush(Color.FromArgb(200, 20, 20, 25)),
                    CornerRadius = new CornerRadius(8),
                    BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 217, 255)),
                    BorderThickness = new Thickness(1),
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = i
                };

                // Add glassmorphism drop shadow
                var shadow = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 270,
                    ShadowDepth = 5,
                    BlurRadius = 15,
                    Opacity = 0.5
                };
                card.Effect = shadow;

                var content = new Grid();
                content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(82) });
                content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var artworkPath = !string.IsNullOrWhiteSpace(game.LogoPath) && System.IO.File.Exists(game.LogoPath)
                    ? game.LogoPath
                    : game.BoxArtPath;

                if (!string.IsNullOrWhiteSpace(artworkPath) && System.IO.File.Exists(artworkPath))
                {
                    try
                    {
                        var artwork = new Image
                        {
                            Source = new BitmapImage(new Uri(artworkPath, UriKind.Absolute)),
                            Stretch = Stretch.Uniform,
                            Margin = new Thickness(8)
                        };
                        Grid.SetColumn(artwork, 0);
                        content.Children.Add(artwork);
                    }
                    catch (Exception)
                    {
                        // A broken optional artwork file must not prevent the library from loading.
                    }
                }

                TextBlock text = new TextBlock
                {
                    Text = game.Title,
                    Foreground = Brushes.White,
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(10)
                };

                Grid.SetColumn(text, 1);
                content.Children.Add(text);
                card.Child = content;
                card.MouseLeftButtonUp += GameCard_MouseLeftButtonUp;

                // Create initial transforms
                TransformGroup transformGroup = new TransformGroup();
                transformGroup.Children.Add(new ScaleTransform(1.0, 1.0));
                transformGroup.Children.Add(new TranslateTransform(0, 0));
                
                card.RenderTransform = transformGroup;

                WheelCanvas.Children.Add(card);
                _gameUIElements.Add(card);
            }
        }

        private void GameCard_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is not Border card || card.Tag is not int index || index < 0 || index >= _games.Count)
                return;

            _selectedIndex = index;
            UpdateLayoutPositions(true);
            GameSelected?.Invoke(this, _games[_selectedIndex]);
            e.Handled = true;
        }

        private void UpdateLayoutPositions(bool animate)
        {
            if (_games.Count == 0 || ActualWidth == 0 || ActualHeight == 0) return;

            for (int i = 0; i < _games.Count; i++)
            {
                var layout = WheelRenderer.CalculateItemLayout(
                    i, 
                    _selectedIndex, 
                    _games.Count, 
                    _currentMode, 
                    ActualWidth, 
                    ActualHeight
                );

                var element = _gameUIElements[i];
                // Renderer coordinates are relative to the viewport center. The canvas
                // itself is stretched, so anchor every card at that center first.
                Canvas.SetLeft(element, Math.Max(0, ActualWidth / 2 - element.Width / 2));
                Canvas.SetTop(element, Math.Max(0, ActualHeight / 2 - element.Height / 2));
                var transformGroup = (TransformGroup)element.RenderTransform;
                var scaleTransform = (ScaleTransform)transformGroup.Children[0];
                var translateTransform = (TranslateTransform)transformGroup.Children[1];

                Canvas.SetZIndex(element, layout.ZIndex);

                if (animate)
                {
                    // Animate properties with premium easing
                    var ease = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 4 };
                    
                    DoubleAnimation xAnim = new DoubleAnimation(layout.X, TimeSpan.FromMilliseconds(_animationDurationMs)) { EasingFunction = ease };
                    DoubleAnimation yAnim = new DoubleAnimation(layout.Y, TimeSpan.FromMilliseconds(_animationDurationMs)) { EasingFunction = ease };
                    DoubleAnimation scaleAnim = new DoubleAnimation(layout.Scale, TimeSpan.FromMilliseconds(_animationDurationMs)) { EasingFunction = ease };
                    DoubleAnimation opacityAnim = new DoubleAnimation(layout.Opacity, TimeSpan.FromMilliseconds(_animationDurationMs)) { EasingFunction = ease };

                    translateTransform.BeginAnimation(TranslateTransform.XProperty, xAnim);
                    translateTransform.BeginAnimation(TranslateTransform.YProperty, yAnim);
                    
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
                    
                    element.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
                    
                    // Highlight selected item with glowing effect
                    if (i == _selectedIndex)
                    {
                        var borderAnim = new ColorAnimation(Colors.White, TimeSpan.FromMilliseconds(_animationDurationMs));
                        element.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, borderAnim);
                        
                        // Add glow
                        var glow = (System.Windows.Media.Effects.DropShadowEffect)element.Effect;
                        var glowAnim = new DoubleAnimation(25, TimeSpan.FromMilliseconds(_animationDurationMs)) { EasingFunction = ease };
                        var glowOpacityAnim = new DoubleAnimation(0.8, TimeSpan.FromMilliseconds(_animationDurationMs)) { EasingFunction = ease };
                        var glowColorAnim = new ColorAnimation((Color)ColorConverter.ConvertFromString("#00D9FF"), TimeSpan.FromMilliseconds(_animationDurationMs));
                        
                        glow.BeginAnimation(System.Windows.Media.Effects.DropShadowEffect.BlurRadiusProperty, glowAnim);
                        glow.BeginAnimation(System.Windows.Media.Effects.DropShadowEffect.OpacityProperty, glowOpacityAnim);
                        glow.BeginAnimation(System.Windows.Media.Effects.DropShadowEffect.ColorProperty, glowColorAnim);
                    }
                    else
                    {
                        var borderAnim = new ColorAnimation(Color.FromArgb(100, 0, 217, 255), TimeSpan.FromMilliseconds(_animationDurationMs));
                        element.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, borderAnim);

                        // Reset glow to default card shadow
                        var glow = (System.Windows.Media.Effects.DropShadowEffect)element.Effect;
                        var glowAnim = new DoubleAnimation(15, TimeSpan.FromMilliseconds(_animationDurationMs));
                        var glowOpacityAnim = new DoubleAnimation(0.5, TimeSpan.FromMilliseconds(_animationDurationMs));
                        var glowColorAnim = new ColorAnimation(Colors.Black, TimeSpan.FromMilliseconds(_animationDurationMs));

                        glow.BeginAnimation(System.Windows.Media.Effects.DropShadowEffect.BlurRadiusProperty, glowAnim);
                        glow.BeginAnimation(System.Windows.Media.Effects.DropShadowEffect.OpacityProperty, glowOpacityAnim);
                        glow.BeginAnimation(System.Windows.Media.Effects.DropShadowEffect.ColorProperty, glowColorAnim);
                    }
                }
                else
                {
                    // Snap instantly
                    translateTransform.X = layout.X;
                    translateTransform.Y = layout.Y;
                    scaleTransform.ScaleX = layout.Scale;
                    scaleTransform.ScaleY = layout.Scale;
                    element.Opacity = layout.Opacity;
                    
                    if (i == _selectedIndex)
                        element.BorderBrush = new SolidColorBrush(Colors.White);
                    else
                        element.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 217, 255));
                }
            }
        }
    }
}
