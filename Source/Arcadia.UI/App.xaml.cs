using System.Windows;
using System;
using System.IO;
using System.Collections.Generic;
using Arcadia.Core.Services;
using Arcadia.UI.Services;
using Arcadia.Core.Data;
using Arcadia.Core.Plugins;
using Arcadia.UI.ViewModels;
using Arcadia.UI.Input;
using Arcadia.UI.Rendering;
using Arcadia.Launchers.TeknoParrot;

namespace Arcadia.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Define local data paths
            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Arcadia");
            Directory.CreateDirectory(appData);

            // Core Services Initialization
            var settingsManager = new SettingsManager(Path.Combine(appData, "settings.json"));
            var db = new GameDatabase(Path.Combine(appData, "games.db"));
            
            var scanner = new GameScannerService(db);
            var tpService = new TeknoParrotService();
            var launcher = new GameLauncher(db);
            var saveManager = new SaveStateManager(db);
            
            var pluginLoader = new PluginLoader();
            var plugins = pluginLoader.LoadPlugins();

            // UI Orchestration
            var viewModel = new GameWheelViewModel(db, launcher);
            var wizard = new SmartWizardService(db, scanner, tpService, saveManager, plugins, settingsManager);

            var mainWindow = new MainWindow(viewModel, wizard, launcher, settingsManager);
            mainWindow.Show();
        }
    }
}