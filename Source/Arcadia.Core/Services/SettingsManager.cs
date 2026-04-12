using System.IO;
using System.Text.Json;
using Arcadia.Core.Models;

namespace Arcadia.Core.Services
{
    public class SettingsManager
    {
        private readonly string _settingsPath;
        public AppSettings Settings { get; private set; } = new AppSettings();

        public SettingsManager(string settingsPath)
        {
            _settingsPath = settingsPath;
            Settings = new AppSettings();
            LoadSettings();
        }

        public void LoadSettings()
        {
            if (File.Exists(_settingsPath))
            {
                try
                {
                    string json = File.ReadAllText(_settingsPath);
                    Settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                catch
                {
                    Settings = new AppSettings();
                }
            }
            else
            {
                Settings = new AppSettings();
                SaveSettings();
            }
        }

        public void SaveSettings()
        {
            try
            {
                string json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsPath, json);
            }
            catch { }
        }
    }
}