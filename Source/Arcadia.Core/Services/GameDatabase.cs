using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Arcadia.Core.Models;
using Newtonsoft.Json;

namespace Arcadia.Core.Services
{
    /// <summary>
    /// Manages the game database using SQLite
    /// </summary>
    public class GameDatabase
    {
        private readonly string _databasePath;
        private readonly string _connectionString;

        public GameDatabase(string databasePath)
        {
            _databasePath = databasePath;
            _connectionString = $"Data Source={databasePath};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            if (!File.Exists(_databasePath))
            {
                SQLiteConnection.CreateFile(_databasePath);
            }

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Games (
                    Id TEXT PRIMARY KEY,
                    Title TEXT NOT NULL,
                    Platform TEXT,
                    Publisher TEXT,
                    Developer TEXT,
                    ReleaseYear INTEGER,
                    Genre TEXT,
                    Description TEXT,
                    ExecutablePath TEXT,
                    RomPath TEXT,
                    BoxArtPath TEXT,
                    CartArtPath TEXT,
                    LogoPath TEXT,
                    FanArtPath TEXT,
                    VideoPreviewPath TEXT,
                    ThemePath TEXT,
                    PlayerCount INTEGER DEFAULT 1,
                    IsFavorite INTEGER DEFAULT 0,
                    PlayTime REAL DEFAULT 0,
                    LastPlayed TEXT,
                    TimesPlayed INTEGER DEFAULT 0,
                    CompletionStatus INTEGER DEFAULT 0,
                    LaunchType INTEGER,
                    EmulatorId TEXT,
                    LauncherId TEXT,
                    CommandLineArgs TEXT,
                    Tags TEXT,
                    Collections TEXT,
                    IsTeknoParrotGame INTEGER DEFAULT 0,
                    TeknoParrotProfilePath TEXT,
                    TeknoParrotType INTEGER DEFAULT 0
                );

                CREATE INDEX IF NOT EXISTS idx_title ON Games(Title);
                CREATE INDEX IF NOT EXISTS idx_platform ON Games(Platform);
                CREATE INDEX IF NOT EXISTS idx_genre ON Games(Genre);
                CREATE INDEX IF NOT EXISTS idx_launch_type ON Games(LaunchType);
            ";

            using var command = new SQLiteCommand(createTableQuery, connection);
            command.ExecuteNonQuery();
        }

        public void AddGame(Game game)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string insertQuery = @"
                INSERT OR REPLACE INTO Games (
                    Id, Title, Platform, Publisher, Developer, ReleaseYear, Genre, Description,
                    ExecutablePath, RomPath, BoxArtPath, CartArtPath, LogoPath, FanArtPath,
                    VideoPreviewPath, ThemePath, PlayerCount, IsFavorite, PlayTime, LastPlayed,
                    TimesPlayed, CompletionStatus, LaunchType, EmulatorId, LauncherId,
                    CommandLineArgs, Tags, Collections, IsTeknoParrotGame, TeknoParrotProfilePath,
                    TeknoParrotType
                ) VALUES (
                    @Id, @Title, @Platform, @Publisher, @Developer, @ReleaseYear, @Genre, @Description,
                    @ExecutablePath, @RomPath, @BoxArtPath, @CartArtPath, @LogoPath, @FanArtPath,
                    @VideoPreviewPath, @ThemePath, @PlayerCount, @IsFavorite, @PlayTime, @LastPlayed,
                    @TimesPlayed, @CompletionStatus, @LaunchType, @EmulatorId, @LauncherId,
                    @CommandLineArgs, @Tags, @Collections, @IsTeknoParrotGame, @TeknoParrotProfilePath,
                    @TeknoParrotType
                )
            ";

            using var command = new SQLiteCommand(insertQuery, connection);
            AddGameParameters(command, game);
            command.ExecuteNonQuery();
        }

        public Game? GetGame(string id)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string selectQuery = "SELECT * FROM Games WHERE Id = @Id";
            using var command = new SQLiteCommand(selectQuery, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return ReadGameFromReader(reader);
            }

            return null;
        }

        public List<Game> GetAllGames()
        {
            var games = new List<Game>();

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string selectQuery = "SELECT * FROM Games ORDER BY Title";
            using var command = new SQLiteCommand(selectQuery, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                games.Add(ReadGameFromReader(reader));
            }

            return games;
        }

        public List<Game> SearchGames(string searchTerm)
        {
            var games = new List<Game>();

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string selectQuery = @"
                SELECT * FROM Games 
                WHERE Title LIKE @SearchTerm 
                   OR Platform LIKE @SearchTerm 
                   OR Genre LIKE @SearchTerm
                   OR Publisher LIKE @SearchTerm
                ORDER BY Title
            ";

            using var command = new SQLiteCommand(selectQuery, connection);
            command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                games.Add(ReadGameFromReader(reader));
            }

            return games;
        }

        public List<Game> GetGamesByPlatform(string platform)
        {
            var games = new List<Game>();

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string selectQuery = "SELECT * FROM Games WHERE Platform = @Platform ORDER BY Title";
            using var command = new SQLiteCommand(selectQuery, connection);
            command.Parameters.AddWithValue("@Platform", platform);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                games.Add(ReadGameFromReader(reader));
            }

            return games;
        }

        public void UpdateGame(Game game)
        {
            AddGame(game); // INSERT OR REPLACE handles updates
        }

        public void DeleteGame(string id)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string deleteQuery = "DELETE FROM Games WHERE Id = @Id";
            using var command = new SQLiteCommand(deleteQuery, connection);
            command.Parameters.AddWithValue("@Id", id);
            command.ExecuteNonQuery();
        }

        private void AddGameParameters(SQLiteCommand command, Game game)
        {
            command.Parameters.AddWithValue("@Id", game.Id);
            command.Parameters.AddWithValue("@Title", game.Title);
            command.Parameters.AddWithValue("@Platform", game.Platform ?? string.Empty);
            command.Parameters.AddWithValue("@Publisher", game.Publisher ?? string.Empty);
            command.Parameters.AddWithValue("@Developer", game.Developer ?? string.Empty);
            command.Parameters.AddWithValue("@ReleaseYear", game.ReleaseYear ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Genre", game.Genre ?? string.Empty);
            command.Parameters.AddWithValue("@Description", game.Description ?? string.Empty);
            command.Parameters.AddWithValue("@ExecutablePath", game.ExecutablePath ?? string.Empty);
            command.Parameters.AddWithValue("@RomPath", game.RomPath ?? string.Empty);
            command.Parameters.AddWithValue("@BoxArtPath", game.BoxArtPath ?? string.Empty);
            command.Parameters.AddWithValue("@CartArtPath", game.CartArtPath ?? string.Empty);
            command.Parameters.AddWithValue("@LogoPath", game.LogoPath ?? string.Empty);
            command.Parameters.AddWithValue("@FanArtPath", game.FanArtPath ?? string.Empty);
            command.Parameters.AddWithValue("@VideoPreviewPath", game.VideoPreviewPath ?? string.Empty);
            command.Parameters.AddWithValue("@ThemePath", game.ThemePath ?? string.Empty);
            command.Parameters.AddWithValue("@PlayerCount", game.PlayerCount);
            command.Parameters.AddWithValue("@IsFavorite", game.IsFavorite ? 1 : 0);
            command.Parameters.AddWithValue("@PlayTime", game.PlayTime);
            command.Parameters.AddWithValue("@LastPlayed", game.LastPlayed?.ToString("o") ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@TimesPlayed", game.TimesPlayed);
            command.Parameters.AddWithValue("@CompletionStatus", (int)game.CompletionStatus);
            command.Parameters.AddWithValue("@LaunchType", (int)game.LaunchType);
            command.Parameters.AddWithValue("@EmulatorId", game.EmulatorId ?? string.Empty);
            command.Parameters.AddWithValue("@LauncherId", game.LauncherId ?? string.Empty);
            command.Parameters.AddWithValue("@CommandLineArgs", game.CommandLineArgs ?? string.Empty);
            command.Parameters.AddWithValue("@Tags", JsonConvert.SerializeObject(game.Tags));
            command.Parameters.AddWithValue("@Collections", JsonConvert.SerializeObject(game.Collections));
            command.Parameters.AddWithValue("@IsTeknoParrotGame", game.IsTeknoParrotGame ? 1 : 0);
            command.Parameters.AddWithValue("@TeknoParrotProfilePath", game.TeknoParrotProfilePath ?? string.Empty);
            command.Parameters.AddWithValue("@TeknoParrotType", (int)game.TeknoParrotType);
        }

        private Game ReadGameFromReader(SQLiteDataReader reader)
        {
            return new Game
            {
                Id = reader.GetString(reader.GetOrdinal("Id")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Platform = reader.GetString(reader.GetOrdinal("Platform")),
                Publisher = reader.GetString(reader.GetOrdinal("Publisher")),
                Developer = reader.GetString(reader.GetOrdinal("Developer")),
                ReleaseYear = reader.IsDBNull(reader.GetOrdinal("ReleaseYear")) ? null : reader.GetInt32(reader.GetOrdinal("ReleaseYear")),
                Genre = reader.GetString(reader.GetOrdinal("Genre")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                ExecutablePath = reader.GetString(reader.GetOrdinal("ExecutablePath")),
                RomPath = reader.GetString(reader.GetOrdinal("RomPath")),
                BoxArtPath = reader.GetString(reader.GetOrdinal("BoxArtPath")),
                CartArtPath = reader.GetString(reader.GetOrdinal("CartArtPath")),
                LogoPath = reader.GetString(reader.GetOrdinal("LogoPath")),
                FanArtPath = reader.GetString(reader.GetOrdinal("FanArtPath")),
                VideoPreviewPath = reader.GetString(reader.GetOrdinal("VideoPreviewPath")),
                ThemePath = reader.GetString(reader.GetOrdinal("ThemePath")),
                PlayerCount = reader.GetInt32(reader.GetOrdinal("PlayerCount")),
                IsFavorite = reader.GetInt32(reader.GetOrdinal("IsFavorite")) == 1,
                PlayTime = reader.GetDouble(reader.GetOrdinal("PlayTime")),
                LastPlayed = reader.IsDBNull(reader.GetOrdinal("LastPlayed")) ? null : DateTime.Parse(reader.GetString(reader.GetOrdinal("LastPlayed"))),
                TimesPlayed = reader.GetInt32(reader.GetOrdinal("TimesPlayed")),
                CompletionStatus = (GameCompletionStatus)reader.GetInt32(reader.GetOrdinal("CompletionStatus")),
                LaunchType = (LaunchType)reader.GetInt32(reader.GetOrdinal("LaunchType")),
                EmulatorId = reader.GetString(reader.GetOrdinal("EmulatorId")),
                LauncherId = reader.GetString(reader.GetOrdinal("LauncherId")),
                CommandLineArgs = reader.GetString(reader.GetOrdinal("CommandLineArgs")),
                Tags = JsonConvert.DeserializeObject<List<string>>(reader.GetString(reader.GetOrdinal("Tags"))) ?? new List<string>(),
                Collections = JsonConvert.DeserializeObject<List<string>>(reader.GetString(reader.GetOrdinal("Collections"))) ?? new List<string>(),
                IsTeknoParrotGame = reader.GetInt32(reader.GetOrdinal("IsTeknoParrotGame")) == 1,
                TeknoParrotProfilePath = reader.GetString(reader.GetOrdinal("TeknoParrotProfilePath")),
                TeknoParrotType = (TeknoParrotGameType)reader.GetInt32(reader.GetOrdinal("TeknoParrotType"))
            };
        }
    }
}
