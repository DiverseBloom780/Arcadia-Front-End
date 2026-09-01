using System.Windows;

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

            // Initialize application settings and services
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}