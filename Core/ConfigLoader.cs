using System.IO;
using System.Xml.Linq;
using Tomlyn;
using YamlDotNet.Serialization;

namespace Arcadia.Core {
    public static class ConfigLoader {
        public static dynamic Load(string path) {
            string ext = Path.GetExtension(path).ToLower();
            string content = File.ReadAllText(path);

            return ext switch {
                ".toml" => Toml.Parse(content).ToModel(),
                ".yaml" or ".yml" => new Deserializer().Deserialize<dynamic>(content),
                ".xml" => XElement.Parse(content),
                _ => throw new InvalidDataException($"Unsupported config format: {ext}")
            };
        }
    }
}
