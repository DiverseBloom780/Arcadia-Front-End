using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Arcadia.Core.Services;
using Arcadia.Updater;

namespace Arcadia.UI.Tabs
{
    public partial class UpdaterTab : UserControl
    {
        private readonly GitHubUpdater? _gitHubUpdater;
        private readonly SettingsManager? _settingsManager;

        public UpdaterTab(GitHubUpdater? gitHubUpdater, SettingsManager? settingsManager)
        {
            InitializeComponent();
            _gitHubUpdater = gitHubUpdater;
            _settingsManager = settingsManager;
        }
        
        public UpdaterTab() : this(null, null)
        {
        }

        private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            if (_gitHubUpdater == null)
            {
                MessageBox.Show("Updater not initialized.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var updateInfo = await _gitHubUpdater.CheckForUpdatesAsync();
                
                if (updateInfo != null)
                {
                    MessageBox.Show(
                        $"Update available: {updateInfo.Version}\n\n{updateInfo.ReleaseNotes}", 
                        "Update Available", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("You are running the latest version.", "Up to Date", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking for updates: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                });
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}