using System;
using System.Diagnostics;
using System.Threading.Tasks;
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

            CurrentVersionText.Text = _settingsManager?.Settings?.General.Version ?? "Unknown";
        }

        // Parameterless constructor for XAML designer
        public UpdaterTab() : this(null, null) { }

        private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            if (_gitHubUpdater == null)
            {
                StatusText.Text = "Error: Updater service is not initialized.";
                return;
            }

            StatusText.Text = "Checking for updates...";
            ReleaseInfoText.Text = "";

            try
            {
                var updateInfo = await _gitHubUpdater.CheckForUpdatesAsync();

                if (updateInfo != null)
                {
                    StatusText.Text = $"Update Available: Version {updateInfo.Version}";
                    ReleaseInfoText.Text = $"Release Notes:\n{updateInfo.ReleaseNotes}";
                }
                else
                {
                    StatusText.Text = "You are running the latest version.";
                    ReleaseInfoText.Text = $"Version {_settingsManager?.Settings?.General.Version ?? "Unknown"} is up to date.";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error checking for updates.";
                ReleaseInfoText.Text = $"Details: {ex.Message}";
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open link: {ex.Message}", "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
