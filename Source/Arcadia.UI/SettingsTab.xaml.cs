using System.Windows.Controls;
using Arcadia.Core.Services;

namespace Arcadia.UI.Tabs
{
    public partial class SettingsTab : UserControl
    {
        private readonly SettingsManager _settingsManager;

        public SettingsTab(SettingsManager settingsManager)
        {
            InitializeComponent();
            _settingsManager = settingsManager;
            DataContext = _settingsManager.Settings;
        }

        public SettingsTab() : this(new SettingsManager("settings.json")) { }
    }
}
