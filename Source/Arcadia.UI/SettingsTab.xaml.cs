using System.Windows.Controls;
using Arcadia.Core.Services; // Required for SettingsManager

namespace Arcadia.UI.Tabs
{
    public partial class SettingsTab : UserControl
    {
        private readonly SettingsManager _settingsManager;

        // Constructor now accepts SettingsManager dependency
        public SettingsTab(SettingsManager settingsManager)
        {
            InitializeComponent();
            _settingsManager = settingsManager;
            // TODO: Add logic here to bind settings properties from _settingsManager
            // to the UI controls defined in SettingsTab.xaml.
        }
        
        // Safety parameterless constructor if needed by XAML preview
        public SettingsTab() : this(null) 
        { 
        }
    }
}