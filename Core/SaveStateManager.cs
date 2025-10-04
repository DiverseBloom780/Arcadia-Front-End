using System;
using System.Collections.Generic;
using System.IO;

namespace Arcadia.Core {
    public static class SaveStateManager {
        private static readonly string SaveRoot = "Saves";

        public static string GetSavePath(string gameName) {
            return Path.Combine(SaveRoot, $"{gameName}.sav");
        }

        public static void Backup(string gameName) {
            var path = GetSavePath(gameName);
            if (File.Exists(path)) {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupPath = Path.Combine(SaveRoot, "Backups", $"{gameName}_{timestamp}.sav");
                Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
                File.Copy(path, backupPath);
            }
        }

        public static void Resume(string gameName) {
            var path = GetSavePath(gameName);
            if (File.Exists(path)) {
                EmulatorManager.Launch(path); // assumes emulator accepts save path as launch arg
            }
        }
    }
}
