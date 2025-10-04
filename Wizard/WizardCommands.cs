using System;
using Arcadia.TeknoParrot;
using Arcadia.Input;
using Arcadia.Media;

namespace Arcadia.Wizard {
    public static class WizardCommands {
        public static void Setup(string[] args) {
            Console.WriteLine("🧙 Setting up Arcadia...");
            var devices = DeviceScanner.ScanDevices();
            Console.WriteLine($"Detected devices: {string.Join(", ", devices)}");
        }

        public static void Repair(string[] args) {
            Console.WriteLine("🔧 Repairing TeknoParrot configs...");
            var valid = TPValidator.Validate("TeknoParrot/GameProfiles.xml", "TeknoParrot/schema.xsd");
            Console.WriteLine(valid ? "✅ All profiles valid." : "⚠️ Validation failed.");
        }

        public static void Config(string[] args) {
            Console.WriteLine("⚙️ Configuring input profiles...");
            var profile = InputMapper.Generate("Daytona USA", GameType.Driving, "Logitech G29");
            Console.WriteLine($"Mapped {profile.Game} to {profile.Device}");
        }

        public static void FetchMedia(string[] args) {
            Console.WriteLine("🎨 Fetching missing media...");
            MediaFetcher.FetchIfMissing("boxart", "Daytona USA", "Assets/Boxart/Daytona USA.png");
        }
    }
}
