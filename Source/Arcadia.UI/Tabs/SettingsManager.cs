using System;
using System.IO;
using Newtonsoft.Json;
using Arcadia.Core.Models;

namespace Arcadia.Core.Services // <--- The correct namespace
{
    public class SettingsManager
    {
        // NOTE: The constructor needs to be updated to accept the file path 
        // to match the usage in MainWindow.xaml.cs.
        // It's currently using a hardcoded path in the old definition.
        private readonly string _settingsFilePath;

        public AppSettings Settings { get; private set; }

        // CHANGE: The constructor must accept a path argument
        public SettingsManager(string settingsFilePath)
        {
            _settingsFilePath = settingsFilePath;
            Settings = LoadSettings() ?? new AppSettings();
        }

        public void SaveSettings()
        {
            try
            {
                var json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
                // Use the member field for the path
                var directory = Path.GetDirectoryName(_settingsFilePath); 
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(_settingsFilePath, json); // Use the member field
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
                if (!File.Exists(_settingsFilePath)) // Use the member field
                    return null;

                var json = File.ReadAllText(_settingsFilePath); // Use the member field
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