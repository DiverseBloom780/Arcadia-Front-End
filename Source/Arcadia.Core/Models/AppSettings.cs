namespace Arcadia.Core.Models
{
    public class AppSettings
    {
        public GeneralSettings General { get; set; } = new GeneralSettings();
        public UISettings UI { get; set; } = new UISettings();
        public GameLibrarySettings GameLibrary { get; set; } = new GameLibrarySettings();
        public InputSettings Input { get; set; } = new InputSettings();
        public UpdateSettings UpdateSettings { get; set; } = new UpdateSettings();
    }

    public class GeneralSettings
    {
        public string ApplicationName { get; set; } = "Arcadia";
        public string Version { get; set; } = "1.0.0";
        public bool StartFullscreen { get; set; } = true;
        public bool CheckForUpdatesOnStartup { get; set; } = true;
    }

    public class UISettings
    {
        public string WheelOrientation { get; set; } = "Vertical";
        public string Theme { get; set; } = "Dark";
        public bool ShowVideoBackgrounds { get; set; } = true;
        public bool ShowGameStats { get; set; } = true;
        public double AnimationSpeed { get; set; } = 1.0;
    }

    public class GameLibrarySettings
    {
        public bool AutoScanOnStartup { get; set; } = true;
        public bool ScanSteam { get; set; } = true;
        public bool ScanGOG { get; set; } = true;
        public bool ScanEpicGames { get; set; } = true;
        public bool ScanTeknoParrot { get; set; } = false;
    }

    public class InputSettings
    {
        public bool KeyboardEnabled { get; set; } = true;
        public bool GamepadEnabled { get; set; } = true;
    }

    public class UpdateSettings
    {
        public string GitHubOwner { get; set; } = "yourusername";
        public string GitHubRepository { get; set; } = "Arcadia";
        public bool CheckForUpdatesOnStartup { get; set; } = true;
        public bool AutoDownloadUpdates { get; set; } = false;
        public bool ShowChangelog { get; set; } = true;
    }
}
