using System;
using System.Windows.Forms;
using Arcadia.UI.Forms; // Ensure ArcadiaLauncherForm is defined in this namespace

namespace Arcadia.UI
{
    internal static class Program
    {
        // This is the static entry point that the compiler is searching for.
        [STAThread]
        static void Main()
        {
            // Initializes configuration for modern .NET Windows Forms apps
            ApplicationConfiguration.Initialize();

            // Optional: Inject runtime config if needed
            // string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "arcadia.runtime.json");
            // ArcadiaConfigLoader.Load(configPath);

            // Optional: Initialize tactile feedback or Smart CLI
            // TactileEngine.Initialize();
            // if (Environment.GetCommandLineArgs().Contains("--wizard")) { SmartWizardCLI.Run(); return; }

            // Starts the application by running your main form/window.
            Application.Run(new ArcadiaLauncherForm());
        }
    }
}
