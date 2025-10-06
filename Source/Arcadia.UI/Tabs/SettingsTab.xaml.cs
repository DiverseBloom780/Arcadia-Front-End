using System.Windows.Controls;
using Arcadia.Core.Services; // Required for SettingsManager

namespace Arcadia.UI.Tabs
{
    public partial class SettingsTab : UserControl
    {
        private readonly SettingsManager _settingsManager;

        // Modified to accept SettingsManager dependency
        public SettingsTab(SettingsManager settingsManager)
        {
            InitializeComponent();
            _settingsManager = settingsManager;
            // TODO: Add logic here to bind settings properties from _settingsManager
            // to the UI controls defined in SettingsTab.xaml.
        }
        
        // Ensure a parameterless constructor is defined if it's used elsewhere (though not by MainWindow now)
        public SettingsTab() : this(null) 
        { 
            // This is a safety constructor, but in this application flow, the one above is primary.
        }
    }
}
