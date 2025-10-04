using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace Arcadia.Core {
    public class GameEntry {
        public string Title { get; set; }
        public string BoxartPath { get; set; }
        public string LogoPath { get; set; }
        public string ExecutablePath { get; set; }
        public string Launcher { get; set; }
    }

    public static class GameLibrary {
        public static List<GameEntry> LoadAll(string path = "Configs/launcher_index.yaml") {
            var yaml = File.ReadAllText(path);
            var deserializer = new Deserializer();
            return deserializer.Deserialize<List<GameEntry>>(yaml);
        }
    }
}
