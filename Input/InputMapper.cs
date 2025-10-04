using System.Collections.Generic;

namespace Arcadia.Input {
    public enum GameType { Driving, Shooting, Fighting }

    public class InputProfile {
        public string Game { get; set; }
        public GameType Type { get; set; }
        public string Device { get; set; }
        public Dictionary<string, string> Bindings { get; set; }
    }

    public static class InputMapper {
        public static InputProfile Generate(string game, GameType type, string device) {
            var bindings = type switch {
                GameType.Driving => new Dictionary<string, string> {
                    { "Accelerate", "Pedal1" },
                    { "Brake", "Pedal2" },
                    { "Steer", "AxisX" }
                },
                GameType.Shooting => new Dictionary<string, string> {
                    { "Aim", "MouseXY" },
                    { "Trigger", "LeftClick" },
                    { "Reload", "RightClick" }
                },
                GameType.Fighting => new Dictionary<string, string> {
                    { "Punch", "Button1" },
                    { "Kick", "Button2" },
                    { "Block", "Button3" }
                },
                _ => new Dictionary<string, string>()
            };

            return new InputProfile {
                Game = game,
                Type = type,
                Device = device,
                Bindings = bindings
            };
        }
    }
}
