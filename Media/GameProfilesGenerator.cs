using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;

namespace Arcadia.TeknoParrot {
    public static class GameProfilesGenerator {
        public static void Generate(List<string> romPaths, string outputPath) {
            var root = new XElement("GameProfiles");

            foreach (var path in romPaths) {
                var exeName = Path.GetFileName(path);
                var gameName = Path.GetFileNameWithoutExtension(path);

                var profile = new XElement("GameProfile",
                    new XElement("Name", gameName),
                    new XElement("Executable", exeName),
                    new XElement("Path", path),
                    new XElement("InputType", "Auto"),
                    new XElement("Launcher", "TeknoParrot")
                );

                root.Add(profile);
            }

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
            doc.Save(outputPath);
        }
    }
}
