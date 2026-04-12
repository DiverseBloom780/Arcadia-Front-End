using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arcadia.Core.Models;
using Arcadia.Core.Data;

namespace Arcadia.Core.Services
{
    /// <summary>
    /// Manages game saves and states across different emulators.
    /// </summary>
    public class SaveStateManager
    {
        private readonly GameDatabase _db;
        private readonly string _backupRoot;

        public SaveStateManager(GameDatabase db)
        {
            _db = db;
            _backupRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Arcadia",
                "Backups",
                "Saves"
            );
            Directory.CreateDirectory(_backupRoot);
        }

        public List<string> GetSaveFiles(Game game)
        {
            if (string.IsNullOrEmpty(game.EmulatorId)) return new List<string>();

            var emulator = _db.GetEmulator(game.EmulatorId);
            if (emulator == null || string.IsNullOrEmpty(emulator.SavePathTemplate)) return new List<string>();

            // Resolve the template path (e.g., {EmulatorDir}\saves\{Title})
            string savePath = ResolveSavePath(emulator.SavePathTemplate, game, emulator);

            if (Directory.Exists(savePath))
            {
                return Directory.GetFiles(savePath, "*.*", SearchOption.AllDirectories).ToList();
            }

            return new List<string>();
        }

        public void CreateBackup(Game game)
        {
            var files = GetSaveFiles(game);
            if (!files.Any()) return;

            string gameBackupDir = Path.Combine(_backupRoot, game.Id, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(gameBackupDir);

            foreach (var file in files)
            {
                string destFile = Path.Combine(gameBackupDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }
        }

        private string ResolveSavePath(string template, Game game, Emulator emulator)
        {
            var emulatorDir = Path.GetDirectoryName(emulator.ExecutablePath) ?? string.Empty;
            
            var result = template
                .Replace("{EmulatorDir}", emulatorDir)
                .Replace("{Title}", game.Title)
                .Replace("{Id}", game.Id)
                .Replace("{RomName}", Path.GetFileNameWithoutExtension(game.RomPath ?? ""));

            return result;
        }
    }
}