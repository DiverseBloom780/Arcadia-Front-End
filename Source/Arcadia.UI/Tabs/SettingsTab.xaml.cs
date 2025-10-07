using System;
using System.IO;
using Newtonsoft.Json;
using Arcadia.Core.Models;

namespace Arcadia.Core.Services
{
    public class SettingsManager
    {
        private const string SettingsFilePath = "Config\\settings.json";

        public AppSettings Settings { get; private set; }

        public SettingsManager()
        {
            Settings = LoadSettings() ?? new AppSettings();
        }

        public void SaveSettings()
        {
            try
            {
                var json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
                var directory = Path.GetDirectoryName(SettingsFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to save settings.", ex);
            }
        }

        private AppSettings? LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                    return null;

                var json = File.ReadAllText(SettingsFilePath);
                return JsonConvert.DeserializeObject<AppSettings>(json);
            }
            catch
            {
                return null;
            }
        }

        public void ResetSettings()
        {
            Settings = new AppSettings();
        }
    }
}
