using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Arcadia.UI.ViewModels;
using Arcadia.UI.Rendering;
using Arcadia.UI.Input;
using Arcadia.Core.Models;
using Arcadia.Core.Services;
using Arcadia.UI.Services;
using SharpDX.D3DCompiler;

namespace Arcadia.UI
{
    public partial class MainWindow : Window
    {
        private readonly GameWheelViewModel _viewModel;
        private readonly SmartWizardService _wizard;
        private readonly D3DRenderHost _renderHost;
        private readonly WheelRenderer _renderer;
        private readonly TextureCache _textureCache;
        private readonly Arcadia.UI.Input.InputManager _inputManager;
        private readonly SettingsManager _settingsManager;

        public MainWindow(GameWheelViewModel viewModel, SmartWizardService wizard, GameLauncher launcher, SettingsManager settingsManager)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _wizard = wizard;
            _settingsManager = settingsManager;
            DataContext = _viewModel;

            // Setup Rendering
            _renderHost = new D3DRenderHost(1920, 1080);
            if (D3DSurface != null && _renderHost != null)
            {
                D3DSurface.Source = _renderHost.ImageSource;
            }
            
            _renderer = new WheelRenderer();
            _textureCache = new TextureCache(_renderHost!.Device);

            InitializeShaders();
            UpdateWheelSettings();

            // Setup Input
            _inputManager = new Arcadia.UI.Input.InputManager(_viewModel);

            CompositionTarget.Rendering += OnCompositionRendering;
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            // If library is empty, it's likely first run. 
            // Automatically introduce the user to Arcadia's privacy principles.
            if (_viewModel != null && _viewModel.Games.Count == 0)
            {
                ShowWizard();
                string response = await _wizard.ProcessCommandAsync("about");
                if (TerminalHistory != null && !string.IsNullOrEmpty(response))
                {
                    TerminalHistory.Text += $"\n{response}";
                }
            }
        }

        private void InitializeShaders()
        {
            // Compile HLSL for the GPU
            using var vsByteCode = ShaderBytecode.CompileFromFile("Shaders.hlsl", "VS", "vs_4_0");
            using var psByteCode = ShaderBytecode.CompileFromFile("Shaders.hlsl", "PS", "ps_4_0");
            
            if (vsByteCode?.Bytecode != null && psByteCode?.Bytecode != null && _renderHost != null)
            {
                _renderer.Initialize(_renderHost.Device, vsByteCode.Bytecode, psByteCode.Bytecode);
            }
        }

        private void OnCompositionRendering(object? sender, EventArgs e)
        {
            _renderHost?.Render((context, rtv) =>
            {
                context.ClearRenderTargetView(rtv, new SharpDX.Color4(0, 0, 0, 0));
                _renderer.Render(context, _viewModel.CurrentScrollOffset, _viewModel.Games, _textureCache);
            });
        }

        private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (SmartWizardOverlay.Visibility == Visibility.Visible) return;

            if (e.Key == Key.F3)
            {
                ShowWizard();
                return;
            }

            _viewModel.HandleKeyDown(e.Key);
        }

        private void ShowWizard()
        {
            if (SmartWizardOverlay != null) SmartWizardOverlay.Visibility = Visibility.Visible;
            TerminalInput?.Focus();
            if (TerminalHistory != null) TerminalHistory.Text = "ARCADIA SMART WIZARD [V1.0]\nType 'about' for info or 'help' for commands.\n---------------------------";
        }

        private async void OnTerminalKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (SmartWizardOverlay != null) SmartWizardOverlay.Visibility = Visibility.Collapsed;
                return;
            }

            if (e.Key == Key.Enter)
            {
                string input = TerminalInput?.Text?.Trim() ?? string.Empty;
                TerminalInput?.Clear();
                
                if (TerminalHistory != null) { TerminalHistory.Text += $"\n> {input}"; }
                string? response = await (_wizard?.ProcessCommandAsync(input) ?? Task.FromResult("Error: Wizard not initialized."));
                if (TerminalHistory != null && !string.IsNullOrEmpty(response)) { TerminalHistory.Text += $"\n{response}"; }
                
                // Auto-refresh wheel if customization commands were used
                if (input.StartsWith("set-wheel") || input.StartsWith("set-tilt") || 
                    input.StartsWith("set-radius") || input.StartsWith("set-spacing") ||
                    input.StartsWith("set-linear") || input.StartsWith("set-x") ||
                    input.StartsWith("set-y") || input.StartsWith("set-logo-size") ||
                    input.StartsWith("set-accent"))
                {
                    UpdateWheelSettings();
                }
            }
        }

        private void UpdateWheelSettings()
        {
            var s = _settingsManager.Settings;
            if (Enum.TryParse<WheelMode>(s.WheelOrientation, true, out var mode))
                _renderer.CurrentMode = mode;

            _renderer.TiltAngle = s.TiltAngle;
            _renderer.WheelRadius = s.WheelRadius;
            _renderer.ItemSpacing = s.ItemSpacing;
            _renderer.LinearSpacing = s.LinearSpacing;
            _renderer.WheelXOffset = s.WheelXOffset;
            _renderer.WheelYOffset = s.WheelYOffset;
            _renderer.LogoWidth = s.LogoWidth;
            _renderer.LogoHeight = s.LogoHeight;

            try {
                var color = (Color)ColorConverter.ConvertFromString(s.AccentColor);
                _renderer.AccentColor = new SharpDX.Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
            } catch { _renderer.AccentColor = SharpDX.Color4.White; }
        }
    }
}