using System.Windows.Controls;
using System.Windows;
using Arcadia.Core.Services;

namespace Arcadia.UI.Tabs
{
    public partial class SettingsTab : UserControl
    {
        private readonly SettingsManager _settingsManager;

        // Constructor now takes one SettingsManager argument
        public SettingsTab(SettingsManager settingsManager)
        {
            InitializeComponent();
            _settingsManager = settingsManager;
            DataContext = _settingsManager.Settings; // Set the settings object as the data context

            // TODO: Implement binding setup here if not using XAML bindings

            // Initial setup for the AnimationSpeedLabel (assuming no binding setup)
            AnimationSpeedLabel.Text = $"{AnimationSpeedSlider.Value:F1}x";
            AnimationSpeedSlider.ValueChanged += AnimationSpeedSlider_ValueChanged;
        }

        private void AnimationSpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AnimationSpeedLabel.Text = $"{e.NewValue:F1}x";
        }


        // Implement the SaveSettings_Click event handler
        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Ensure all UI controls are properly bound or read into _settingsManager.Settings
            _settingsManager.SaveSettings();
            MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Implement the ResetSettings_Click event handler
        private void ResetSettings_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset all settings to default?", "Confirm Reset", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _settingsManager.ResetSettings();
                // Re-apply the context or reload the tab to refresh UI
                DataContext = null;
                DataContext = _settingsManager.Settings;
                
                MessageBox.Show("Settings reset to defaults.", "Reset Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}