using System;
using System.IO;
using Newtonsoft.Json;
using Arcadia.Core.Models;

namespace Arcadia.Core.Services
{
    public class SettingsManager
    {
        private readonly string _settingsFilePath;
<<<<<<< HEAD

        public AppSettings Settings { get; private set; }
=======
        public AppSettings Settings { get; private set; } = new AppSettings();
>>>>>>> 282f32e (Overhaul Arcadia frontend and filter redistributables)

        public SettingsManager(string settingsFilePath)
        {
            _settingsFilePath = settingsFilePath;
            Settings = LoadSettings();
        }

        private AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                // Optional: log error or fallback
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }

            return new AppSettings();
        }

        public void SaveSettings()
        {
            try
            {
                string json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
                var directory = Path.GetDirectoryName(_settingsFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to save settings.", ex);
            }
        }

        public void ResetSettings()
        {
            Settings = new AppSettings();
        }
    }
}
