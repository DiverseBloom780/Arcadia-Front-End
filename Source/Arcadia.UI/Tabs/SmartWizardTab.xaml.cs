using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Arcadia.SmartWizard;

namespace Arcadia.UI.Tabs
{
    public partial class SmartWizardTab : UserControl
    {
        private readonly SmartWizardService _wizardService;

        public SmartWizardTab(SmartWizardService wizardService)
        {
            InitializeComponent();
            _wizardService = wizardService;
            LoadSuggestions();
        }

        private void LoadSuggestions()
        {
            var suggestions = _wizardService.GetSuggestions();
            SuggestionsList.ItemsSource = suggestions;
            
            if (suggestions.Count == 0)
            {
                // TODO: Add an "Everything looks good" empty state
            }
        }

        private async void FixButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is WizardSuggestion suggestion)
            {
                btn.IsEnabled = false;
                bool success = await _wizardService.ApplyFix(suggestion);
                
                if (success)
                {
                    MessageBox.Show($"Successfully resolved: {suggestion.Title}", "Smart Wizard", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadSuggestions(); // Refresh the list
                }
                else
                {
                    MessageBox.Show($"Failed to resolve: {suggestion.Title}", "Smart Wizard", MessageBoxButton.OK, MessageBoxImage.Warning);
                    btn.IsEnabled = true;
                }
            }
        }
    }
}
