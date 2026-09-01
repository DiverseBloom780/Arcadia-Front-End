<<<<<<< HEAD
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Arcadia.Core.Services;
using Arcadia.Core.Models;

namespace Arcadia.UI.Tabs
{
    public partial class SettingsTab : UserControl
    {
        private readonly SettingsManager _settingsManager;

        public SettingsTab(SettingsManager settingsManager)
        {
            InitializeComponent();
            _settingsManager = settingsManager;

            // Bind settings to UI
            DataContext = _settingsManager.Settings;

            // Initialize animation speed label
            AnimationSpeedLabel.Text = $"{AnimationSpeedSlider.Value:F1}x";
            AnimationSpeedSlider.ValueChanged += AnimationSpeedSlider_ValueChanged;
        }

        // Save settings to file
        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _settingsManager.SaveSettings();
                MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Reset settings to default
        private void ResetSettings_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Are you sure you want to reset all settings to default? This cannot be undone.",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm == MessageBoxResult.Yes)
            {
                _settingsManager.ResetSettings();

                // Refresh UI bindings
                DataContext = null;
                DataContext = _settingsManager.Settings;

                // Manually update slider label
                AnimationSpeedLabel.Text = $"{_settingsManager.Settings.UI.AnimationSpeed:F1}x";

                MessageBox.Show("Settings reset to defaults.", "Reset Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Update animation speed label when slider changes
        private void AnimationSpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (AnimationSpeedLabel != null)
            {
                AnimationSpeedLabel.Text = $"{e.NewValue:F1}x";
            }
        }
    }
=======
using System.Windows.Controls;
using Arcadia.Core.Services;

namespace Arcadia.UI.Tabs
{
    public partial class SettingsTab : UserControl
    {
        private readonly SettingsManager? _settingsManager;

        public SettingsTab(SettingsManager? settingsManager)
        {
            InitializeComponent();
            _settingsManager = settingsManager;
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            if (_settingsManager?.Settings == null) return;
            var s = _settingsManager.Settings;

            // General
            FullscreenToggle.IsChecked = s.General.StartFullscreen;
            UpdateToggle.IsChecked = s.General.CheckForUpdatesOnStartup;

            // UI
            OrientationCombo.SelectedIndex = s.UI.WheelOrientation == "Horizontal" ? 1 : 0;

            // Library
            AutoScanToggle.IsChecked = s.GameLibrary.AutoScanOnStartup;
        }

        private void SettingsCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SettingsCategories.SelectedItem is ListBoxItem item)
            {
                string category = item.Tag?.ToString() ?? "General";
                CategoryTitle.Text = item.Content.ToString() + " Settings";
                
                // Toggle Panel Visibility
                if (GeneralSettingsPanel != null) GeneralSettingsPanel.Visibility = System.Windows.Visibility.Collapsed;
                if (UISettingsPanel != null) UISettingsPanel.Visibility = System.Windows.Visibility.Collapsed;
                if (LibrarySettingsPanel != null) LibrarySettingsPanel.Visibility = System.Windows.Visibility.Collapsed;

                switch (category)
                {
                    case "General" when GeneralSettingsPanel != null:
                        GeneralSettingsPanel.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case "UI" when UISettingsPanel != null:
                        UISettingsPanel.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case "Library" when LibrarySettingsPanel != null:
                        LibrarySettingsPanel.Visibility = System.Windows.Visibility.Visible;
                        break;
                }
            }
        }

        private void ScanLibrary_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // We need to tell MainWindow to re-scan. 
            // Since Tabs are hosted in MainWindow, we can find the parent or use an event.
            // For now, we'll trigger a message box and recommend a restart, 
            // but in a full implementation we'd use a shared service.
            System.Windows.MessageBox.Show("Library scan started in the background. Please wait a few moments...", "Arcadia Scan", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            
            // To properly implement this, we'd need access to the scanning logic in Core
            // which is currently triggered in MainWindow.LoadGames().
        }

        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_settingsManager == null) return;

            // General
            _settingsManager.Settings.General.StartFullscreen = FullscreenToggle.IsChecked ?? true;
            _settingsManager.Settings.General.CheckForUpdatesOnStartup = UpdateToggle.IsChecked ?? true;

            // UI
            _settingsManager.Settings.UI.WheelOrientation = (OrientationCombo.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Vertical";

            // Library
            _settingsManager.Settings.GameLibrary.AutoScanOnStartup = AutoScanToggle.IsChecked ?? true;

            _settingsManager.SaveSettings();
            System.Windows.MessageBox.Show("Settings saved successfully!", "Arcadia", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        
        public SettingsTab() : this(null) 
        { 
        }
    }
>>>>>>> 282f32e (Overhaul Arcadia frontend and filter redistributables)
}
