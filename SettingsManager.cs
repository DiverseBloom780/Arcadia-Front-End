using System.IO;
using Newtonsoft.Json;
using Arcadia.Core.Models;

namespace Arcadia.Core.Services
{
    public class SettingsManager
    {
        private readonly string _settingsFilePath;
        public AppSettings Settings { get; private set; }

        public SettingsManager(string settingsFilePath)
        {
            _settingsFilePath = settingsFilePath;
            Settings = LoadSettings();
        }

        private AppSettings LoadSettings()
        {
            if (File.Exists(_settingsFilePath))
            {
                string json = File.ReadAllText(_settingsFilePath);
                return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }
            return new AppSettings();
        }

        public void SaveSettings()
        {
            string json = JsonConvert.SerializeObject(Settings, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(_settingsFilePath, json);
        }
    }

    public class AppSettings
    {
        public GeneralSettings General { get; set; } = new GeneralSettings();
        public UISettings UI { get; set; } = new UISettings();
        public GameLibrarySettings GameLibrary { get; set; } = new GameLibrarySettings();
        public EmulatorsSettings Emulators { get; set; } = new EmulatorsSettings();
        public TeknoParrotSettings TeknoParrot { get; set; } = new TeknoParrotSettings();
        public MediaSettings Media { get; set; } = new MediaSettings();
        public UpdateSettings UpdateSettings { get; set; } = new UpdateSettings();
        public SmartWizardSettings SmartWizard { get; set; } = new SmartWizardSettings();
        public InputSettings Input { get; set; } = new InputSettings();
        public AdvancedSettings Advanced { get; set; } = new AdvancedSettings();
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
        public double AnimationSpeed { get; set; } = 1.0;
        public bool ShowGameStats { get; set; } = true;
    }

    public class GameLibrarySettings
    {
        public string DatabasePath { get; set; } = "%AppData%\\Arcadia\\games.db";
        public string MediaCachePath { get; set; } = "%AppData%\\Arcadia\\Media";
        public bool AutoScanOnStartup { get; set; } = true;
        public bool ScanSteam { get; set; } = true;
        public bool ScanGOG { get; set; } = true;
        public bool ScanEpicGames { get; set; } = true;
        public bool ScanTeknoParrot { get; set; } = false;
    }

    public class EmulatorsSettings
    {
        public string EmulatorConfigPath { get; set; } = "%AppData%\\Arcadia\\Emulators";
        public bool AutoDownloadEmulators { get; set; } = false;
        public string BiosPath { get; set; } = "%AppData%\\Arcadia\\BIOS";
    }

    public class TeknoParrotSettings
    {
        public string TeknoParrotPath { get; set; } = "C:\\TeknoParrot";
        public string RomsFolderPath { get; set; } = "C:\\TeknoParrot\\ROMs";
        public string GameProfilesPath { get; set; } = "C:\\TeknoParrot\\GameProfiles";
        public bool AutoGenerateProfiles { get; set; } = true;
        public bool AutoDetectDevices { get; set; } = true;
        public bool EnableForceFeeback { get; set; } = true;
    }

    public class MediaSettings
    {
        public bool AutoDownloadMedia { get; set; } = true;
        public bool DownloadBoxArt { get; set; } = true;
        public bool DownloadLogos { get; set; } = true;
        public bool DownloadCartArt { get; set; } = true;
        public bool DownloadFanArt { get; set; } = true;
        public bool DownloadVideoPreview { get; set; } = true;
        public string PreferredImageFormat { get; set; } = "PNG";
        public int MaxImageWidth { get; set; } = 1920;
        public int MaxImageHeight { get; set; } = 1080;
    }

    public class UpdateSettings
    {
        public string GitHubOwner { get; set; } = "yourusername";
        public string GitHubRepository { get; set; } = "Arcadia";
        public bool CheckForUpdatesOnStartup { get; set; } = true;
        public bool AutoDownloadUpdates { get; set; } = false;
        public bool ShowChangelog { get; set; } = true;
    }

    public class SmartWizardSettings
    {
        public bool Enabled { get; set; } = true;
        public bool LocalMode { get; set; } = true;
        public string KnowledgeBasePath { get; set; } = "%AppData%\\Arcadia\\SmartWizard";
    }

    public class InputSettings
    {
        public bool KeyboardEnabled { get; set; } = true;
        public bool GamepadEnabled { get; set; } = true;
        public int GamepadIndex { get; set; } = 0;
        public string NavigateUpKey { get; set; } = "Up";
        public string NavigateDownKey { get; set; } = "Down";
        public string NavigateLeftKey { get; set; } = "Left";
        public string NavigateRightKey { get; set; } = "Right";
        public string LaunchKey { get; set; } = "Enter";
        public string BackKey { get; set; } = "Escape";
        public string SettingsKey { get; set; } = "F1";
        public string SearchKey { get; set; } = "F2";
        public string SmartWizardKey { get; set; } = "F3";
    }

    public class AdvancedSettings
    {
        public bool EnableLogging { get; set; } = true;
        public string LogLevel { get; set; } = "Info";
        public string LogPath { get; set; } = "%AppData%\\Arcadia\\Logs";
        public bool EnableTelemetry { get; set; } = false;
        public int CacheSizeLimit { get; set; } = 10240;
    }
}
