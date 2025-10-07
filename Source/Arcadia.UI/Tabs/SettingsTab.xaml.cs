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
}
