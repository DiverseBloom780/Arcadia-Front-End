using System;
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
            string json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
            var directory = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_settingsFilePath, json);
        }

        public void ResetSettings()
        {
            Settings = new AppSettings();
        }
    }
}
