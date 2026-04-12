using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arcadia.Core.Models;
using SQLite;

namespace Arcadia.Core.Data
{
    /// <summary>
    /// Manages SQLite operations for storing and retrieving game data.
    /// </summary>
    public class GameDatabase : IDisposable
    {
        private readonly SQLiteConnection _db;

        public GameDatabase(string dbPath)
        {
            string? directory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            _db = new SQLiteConnection(dbPath);
            InitializeTables();
        }

        public void InitializeTables()
        {
            _db.CreateTable<Game>();
            _db.CreateTable<Emulator>();
        }

        public List<Game> GetGames()
        {
            return _db.Table<Game>().ToList();
        }

        public List<Game> GetAllGames()
        {
            return GetGames();
        }

        public List<Game> SearchGames(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm)) return GetGames();

            return _db.Table<Game>()
                .Where(g => g.Title.Contains(searchTerm) || 
                            (g.Platform != null && g.Platform.Contains(searchTerm)) || 
                            (g.Genre != null && g.Genre.Contains(searchTerm)))
                .OrderBy(g => g.Title)
                .ToList();
        }

        public List<Emulator> GetEmulators()
        {
            return _db.Table<Emulator>().ToList();
        }

        public Game? GetGame(string id)
        {
            return _db.Table<Game>().FirstOrDefault(g => g.Id == id);
        }

        public Emulator? GetEmulator(string id)
        {
            return _db.Table<Emulator>().FirstOrDefault(e => e.Id == id);
        }

        public void AddOrUpdateGames(IEnumerable<Game> games)
        {
            if (games == null) return;
            foreach (var game in games)
            {
                if (game == null) continue;
                var existing = GetGame(game.Id);
                if (existing == null)
                {
                    _db.Insert(game);
                }
                else
                {
                    // Preserve user-modified stats when updating from scanner
                    game.PlayTime = existing.PlayTime;
                    game.LastPlayed = existing.LastPlayed;
                    game.TimesPlayed = existing.TimesPlayed;
                    _db.Update(game);
                }
            }
        }

        public void UpdateGame(Game game)
        {
            _db.Update(game);
        }

        public void AddOrUpdateEmulator(Emulator emulator)
        {
            var existing = GetEmulator(emulator.Id);
            if (existing == null)
            {
                _db.Insert(emulator);
            }
            else
            {
                _db.Update(emulator);
            }
        }

        public void DeleteGame(string id)
        {
            _db.Delete<Game>(id);
        }

        public void Dispose()
        {
            _db.Close();
            _db.Dispose();
        }
    }
}