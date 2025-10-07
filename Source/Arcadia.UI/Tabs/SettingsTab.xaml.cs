using System;
using System.Windows;
using System.Windows.Controls;
using Arcadia.Core.Services;
using Arcadia.Core.Models;

namespace Arcadia.UI.Tabs
{
    public partial class SettingsTab : UserControl
    {
        private readonly SettingsManager? _settingsManager;

        public SettingsTab(SettingsManager? settingsManager)
        {
            InitializeComponent();
            _settingsManager = settingsManager;

            LoadSettings();

            // Bind animation speed slider to label
            AnimationSpeedSlider.ValueChanged += (s, e) =>
            {
                AnimationSpeedLabel.Text = $"{e.NewValue:F1}x";
            };
        }

        public SettingsTab() : this(null)
        {
        }

        private void LoadSettings()
        {
            if (_settingsManager?.Settings == null) return;

            var settings = _settingsManager.Settings;

            // General Settings
            ApplicationNameTextBox.Text = settings.General.ApplicationName;
            VersionTextBox.Text = settings.General.Version;
            StartFullscreenCheckBox.IsChecked = settings.General.StartFullscreen;
            CheckForUpdatesCheckBox.IsChecked = settings.General.CheckForUpdatesOnStartup;

            // UI Settings
            WheelOrientationComboBox.SelectedIndex = settings.UI.WheelOrientation switch
            {
                "Vertical" => 0,
                "Horizontal" => 1,
                "Curved" => 2,
                _ => 0
            };
            ThemeComboBox.SelectedIndex = settings.UI.Theme == "Dark" ? 0 : 1;
            ShowVideoBackgroundsCheckBox.IsChecked = settings.UI.ShowVideoBackgrounds;
            ShowGameStatsCheckBox.IsChecked = settings.UI.ShowGameStats;
            AnimationSpeedSlider.Value = settings.UI.AnimationSpeed;
            AnimationSpeedLabel.Text = $"{settings.UI.AnimationSpeed:F1}x";

            // Game Library Settings
            AutoScanCheckBox.IsChecked = settings.GameLibrary.AutoScanOnStartup;
            ScanSteamCheckBox.IsChecked = settings.GameLibrary.ScanSteam;
            ScanGOGCheckBox.IsChecked = settings.GameLibrary.ScanGOG;
            ScanEpicCheckBox.IsChecked = settings.GameLibrary.ScanEpicGames;
            ScanTeknoParrotCheckBox.IsChecked = settings.GameLibrary.ScanTeknoParrot;

            // Input Settings
            KeyboardEnabledCheckBox.IsChecked = settings.Input.KeyboardEnabled;
            GamepadEnabledCheckBox.IsChecked = settings.Input.GamepadEnabled;
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            if (_settingsManager?.Settings == null)
            {
                MessageBox.Show("Settings manager not initialized.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var settings = _settingsManager.Settings;

                // General Settings
                settings.General.ApplicationName = ApplicationNameTextBox.Text;
                settings.General.StartFullscreen = StartFullscreenCheckBox.IsChecked ?? true;
                settings.General.CheckForUpdatesOnStartup = CheckForUpdatesCheckBox.IsChecked ?? true;

                // UI Settings
                settings.UI.WheelOrientation = WheelOrientationComboBox.SelectedIndex switch
                {
                    0 => "Vertical",
                    1 => "Horizontal",
                    2 => "Curved",
                    _ => "Vertical"
                };
                settings.UI.Theme = ThemeComboBox.SelectedIndex == 0 ? "Dark" : "Light";
                settings.UI.ShowVideoBackgrounds = ShowVideoBackgroundsCheckBox.IsChecked ?? true;
                settings.UI.ShowGameStats = ShowGameStatsCheckBox.IsChecked ?? true;
                settings.UI.AnimationSpeed = AnimationSpeedSlider.Value;

                // Game Library Settings
                settings.GameLibrary.AutoScanOnStartup = AutoScanCheckBox.IsChecked ?? true;
                settings.GameLibrary.ScanSteam = ScanSteamCheckBox.IsChecked ?? true;
                settings.GameLibrary.ScanGOG = ScanGOGCheckBox.IsChecked ?? true;
                settings.GameLibrary.ScanEpicGames = ScanEpicCheckBox.IsChecked ?? true;
                settings.GameLibrary.ScanTeknoParrot = ScanTeknoParrotCheckBox.IsChecked ?? false;

                // Input Settings
                settings.Input.KeyboardEnabled = KeyboardEnabledCheckBox.IsChecked ?? true;
                settings.Input.GamepadEnabled = GamepadEnabledCheckBox.IsChecked ?? true;

                // Save to file
                _settingsManager.SaveSettings();

                MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetSettings_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to reset all settings to their default values?",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes && _settingsManager != null)
            {
                _settingsManager.Settings = new AppSettings(); // ✅ Now writable
                _settingsManager.SaveSettings();
                LoadSettings();

                MessageBox.Show("Settings reset to defaults!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
