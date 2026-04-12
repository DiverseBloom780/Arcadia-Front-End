using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Arcadia.Core.Models;

namespace Arcadia.UI.Controls
{
    public enum WheelOrientation { Vertical, Horizontal, Curved }

    public partial class GameWheel : UserControl
    {
        public WheelOrientation Orientation { get; set; } = WheelOrientation.Curved;
        public double Zoom { get; set; } = 1.0;
        public double TargetZoom { get; set; } = 1.0;
        
        private List<object> _items = new List<object>();
        private readonly List<FrameworkElement> _elementPool = new List<FrameworkElement>();
        private readonly List<FrameworkElement> _activeElements = new List<FrameworkElement>();

        private double _offset = 0;
        private double _targetOffset = 0;
        private double _velocity = 0;
        private const double Friction = 0.90;
        private const double SnapStrength = 0.18;
        
        private MediaPlayer _scrollSound = new MediaPlayer();
        private MediaPlayer _selectSound = new MediaPlayer();
        
        public event EventHandler<object>? SelectionChanged;

        public GameWheel()
        {
            InitializeComponent();
            this.Loaded += GameWheel_Loaded;
            this.KeyDown += GameWheel_KeyDown;
            CompositionTarget.Rendering += OnRendering;
        }

        private void GameWheel_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
            RenderWheel();
        }

        private void OnRendering(object sender, EventArgs e)
        {
            if (_items.Count == 0) return;

            _velocity *= Friction;
            double snapForce = (_targetOffset - _offset) * SnapStrength;
            _velocity += snapForce;
            _offset += _velocity;

            // Smooth zoom
            Zoom += (TargetZoom - Zoom) * 0.1;

            RenderWheel();
        }

        public void SetItems<T>(List<T> items) where T : class
        {
            _items = items.Cast<object>().ToList();
            _offset = 0;
            _targetOffset = 0;
            _velocity = 0;
            _lastReportedIndex = -1;
            
            // Return all active elements to pool
            ClearActiveElements();
            
            RenderWheel();
            OnSelectionChanged();
        }

        private void ClearActiveElements()
        {
            foreach (var el in _activeElements)
            {
                el.Visibility = Visibility.Collapsed;
                _elementPool.Add(el);
            }
            _activeElements.Clear();
        }

        private void RenderWheel()
        {
            if (_items.Count == 0) 
            {
                ClearActiveElements();
                return;
            }

            double canvasWidth = WheelCanvas.ActualWidth;
            double canvasHeight = WheelCanvas.ActualHeight;
            if (canvasHeight <= 0) canvasHeight = 1080;
            if (canvasWidth <= 0) canvasWidth = 400;

            int itemsToDraw = 17;
            int halfItems = itemsToDraw / 2;

            var neededIndices = new List<int>();
            for (int i = -halfItems; i <= halfItems; i++)
            {
                int itemIndex = (int)(Math.Round(_offset) + i) % _items.Count;
                if (itemIndex < 0) itemIndex += _items.Count;
                neededIndices.Add(itemIndex);
            }

            // Reuse elements or get from pool
            int activePtr = 0;
            for (int i = -halfItems; i <= halfItems; i++)
            {
                double itemLogicalPos = i + Math.Round(_offset) - _offset;
                int itemIndex = neededIndices[i + halfItems];

                var itemData = _items[itemIndex];
                string title = GetItemTitle(itemData);
                string logoPath = GetItemLogo(itemData);

                double distFromCenter = Math.Abs(itemLogicalPos);
                
                FrameworkElement visualItem;
                if (activePtr < _activeElements.Count)
                {
                    visualItem = _activeElements[activePtr++];
                    UpdateWheelItem((Border)visualItem, title, logoPath);
                }
                else if (_elementPool.Count > 0)
                {
                    visualItem = _elementPool[0];
                    _elementPool.RemoveAt(0);
                    _activeElements.Add(visualItem);
                    activePtr++;
                    UpdateWheelItem((Border)visualItem, title, logoPath);
                    if (!WheelCanvas.Children.Contains(visualItem))
                        WheelCanvas.Children.Add(visualItem);
                    visualItem.Visibility = Visibility.Visible;
                }
                else
                {
                    visualItem = (FrameworkElement)CreateWheelItem(title, logoPath, 1.0, 0);
                    _activeElements.Add(visualItem);
                    activePtr++;
                    WheelCanvas.Children.Add(visualItem);
                    visualItem.Visibility = Visibility.Visible;
                }

                PositionItem(visualItem, itemLogicalPos, distFromCenter, canvasWidth, canvasHeight, halfItems);
            }

            // Collapse remaining active elements
            while (_activeElements.Count > itemsToDraw)
            {
                var el = _activeElements[_activeElements.Count - 1];
                el.Visibility = Visibility.Collapsed;
                _elementPool.Add(el);
                _activeElements.RemoveAt(_activeElements.Count - 1);
            }
        }

        private void PositionItem(FrameworkElement visualItem, double itemLogicalPos, double distFromCenter, double canvasWidth, double canvasHeight, int halfItems)
        {
            double xPos = 0, yPos = 0, rotation = 0, scale = 1.0;
            
            if (Orientation == WheelOrientation.Vertical)
            {
                xPos = 20;
                yPos = (canvasHeight / 2) + (itemLogicalPos * 140);
                rotation = 0;
            }
            else if (Orientation == WheelOrientation.Horizontal)
            {
                xPos = (canvasWidth / 2) + (itemLogicalPos * 300);
                yPos = (canvasHeight / 2) - 60;
                rotation = 0;
            }
            else // Curved
            {
                yPos = (canvasHeight / 2) + (itemLogicalPos * 150);
                xPos = 20 + (Math.Pow(distFromCenter, 2) * 35);
                rotation = itemLogicalPos * -15;
            }

            scale = (1.0 - (distFromCenter * 0.12)) * Zoom;
            if (scale < 0.1) scale = 0.1;
            
            double opacity = 1.0 - (distFromCenter * 0.18);
            if (opacity < 0) opacity = 0;

            visualItem.Opacity = opacity;
            Panel.SetZIndex(visualItem, (int)((1.0 - distFromCenter / (halfItems + 1)) * 100));

            if (Orientation == WheelOrientation.Horizontal)
                Canvas.SetLeft(visualItem, xPos - (175 * scale));
            else
                Canvas.SetRight(visualItem, xPos);

            Canvas.SetTop(visualItem, yPos - (60 * scale));

            var border = (Border)visualItem;
            border.Width = 350 * scale;
            border.Height = 120 * scale;

            var tg = (TransformGroup)border.RenderTransform;
            var rt = (RotateTransform)tg.Children[0];
            rt.Angle = rotation;

            if (distFromCenter < 0.5)
            {
                border.BorderBrush = Brushes.Cyan;
                border.BorderThickness = new Thickness(3);
                border.Effect = new System.Windows.Media.Effects.DropShadowEffect 
                { 
                    Color = Colors.Cyan, 
                    BlurRadius = 30, 
                    ShadowDepth = 0,
                    Opacity = 0.8
                };
            }
            else
            {
                border.BorderBrush = Brushes.LightGray;
                border.BorderThickness = new Thickness(2);
                border.Effect = null;
            }
        }

        private void UpdateWheelItem(Border border, string title, string logoPath)
        {
            var text = (TextBlock)border.Tag; // Store textblock in tag for quick access
            if (text.Text != title) text.Text = title;

            // Simple cache check for image
            var logo = border.Child as Image;
            if (logoPath != (string)border.DataContext)
            {
                border.DataContext = logoPath;
                if (!string.IsNullOrEmpty(logoPath) && System.IO.File.Exists(logoPath))
                {
                    if (logo == null)
                    {
                        logo = new Image { Stretch = Stretch.Uniform, Margin = new Thickness(5) };
                        border.Child = logo;
                    }
                    logo.Source = new BitmapImage(new Uri(logoPath));
                    logo.Visibility = Visibility.Visible;
                    text.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (logo != null) logo.Visibility = Visibility.Collapsed;
                    text.Visibility = Visibility.Visible;
                    border.Child = text;
                }
            }
        }

        private UIElement CreateWheelItem(string title, string logoPath, double scale, double rotation)
        {
            var border = new Border
            {
                Width = 350 * scale,
                Height = 120 * scale,
                Background = new SolidColorBrush(Color.FromArgb(200, 10, 10, 10)),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(12),
                ClipToBounds = true,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new RotateTransform(rotation));
            border.RenderTransform = transformGroup;

            var text = new TextBlock
            {
                Text = title,
                Foreground = Brushes.White,
                FontSize = 26, // Scaled during PositionItem
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(15)
            };

            border.Tag = text; // Cache for performance
            border.Child = text;
            UpdateWheelItem(border, title, logoPath);

            return border;
        }

        private string GetItemTitle(object item)
        {
            if (item is Game g) return g.Title;
            if (item is Platform p) return p.Title;
            return item.ToString() ?? "";
        }

        private string GetItemLogo(object item)
        {
            if (item is Game g) return g.LogoPath;
            if (item is Platform p) return p.LogoPath;
            return "";
        }

        private int _lastReportedIndex = -1;

        public void InputHit(bool up)
        {
            if (up) _targetOffset--;
            else _targetOffset++;
            PlayScrollSound();
            CheckSelectionChanged();
        }

        private void GameWheel_KeyDown(object sender, KeyEventArgs e)
        {
            if (_items.Count == 0) return;

            // Handle Alphabetical Jumps
            if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                JumpToLetter(e.Key.ToString());
                return;
            }

            if (e.Key == Key.Up)
            {
                _targetOffset--;
                PlayScrollSound();
                CheckSelectionChanged();
            }
            else if (e.Key == Key.Down)
            {
                _targetOffset++;
                PlayScrollSound();
                CheckSelectionChanged();
            }
        }

        private void JumpToLetter(string letter)
        {
            int index = _items.FindIndex(item => 
                GetItemTitle(item).TrimStart().StartsWith(letter, StringComparison.OrdinalIgnoreCase));

            if (index != -1)
            {
                // Calculate short path around the circle
                double currentOffset = _targetOffset % _items.Count;
                if (currentOffset < 0) currentOffset += _items.Count;
                
                double diff = index - currentOffset;
                if (diff > _items.Count / 2) diff -= _items.Count;
                else if (diff < -_items.Count / 2) diff += _items.Count;
                
                _targetOffset += diff;
                PlayScrollSound();
                CheckSelectionChanged();
            }
        }

        private void PlayScrollSound()
        {
            try { _scrollSound.Stop(); _scrollSound.Play(); } catch { }
        }

        private void PlaySelectSound()
        {
            try { _selectSound.Stop(); _selectSound.Play(); } catch { }
        }

        private void CheckSelectionChanged()
        {
            int index = (int)Math.Round(_targetOffset) % _items.Count;
            if (index < 0) index += _items.Count;
            
            if (index != _lastReportedIndex)
            {
                _lastReportedIndex = index;
                OnSelectionChanged();
            }
        }

        private void OnSelectionChanged()
        {
            int index = (int)Math.Round(_targetOffset) % _items.Count;
            if (index < 0) index += _items.Count;
            
            if (index >= 0 && index < _items.Count)
            {
                SelectionChanged?.Invoke(this, _items[index]);
            }
        }
    }
}
