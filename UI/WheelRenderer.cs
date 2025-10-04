using System;
using System.Collections.Generic;
using System.Numerics;
using Arcadia.Core;
using Arcadia.Media;

namespace Arcadia.UI {
    public enum WheelDirection { Vertical, Horizontal, Curved }

    public class WheelRenderer {
        private WheelDirection direction;
        private float animationSpeed;
        private List<GameEntry> games;
        private int selectedIndex;
        private float scrollOffset;

        public WheelRenderer(string configPath) {
            var config = ConfigLoader.Load(configPath);
            direction = Enum.Parse<WheelDirection>(config["wheel"]["direction"].ToString(), true);
            animationSpeed = float.Parse(config["wheel"]["animation_speed"].ToString());
            games = GameLibrary.LoadAll();
            selectedIndex = 0;
            scrollOffset = 0f;
        }

        public void Update(float deltaTime, int inputDelta) {
            scrollOffset += inputDelta * animationSpeed * deltaTime;
            selectedIndex = Math.Clamp(selectedIndex + inputDelta, 0, games.Count - 1);
        }

        public void Render() {
            for (int i = 0; i < games.Count; i++) {
                var offset = i - selectedIndex;
                var position = GetWheelPosition(offset);
                var scale = GetScale(offset);
                var game = games[i];

                ThemeManager.DrawBoxart(game.BoxartPath, position, scale);
                ThemeManager.DrawLogo(game.LogoPath, position + new Vector2(0, 80), scale * 0.8f);
            }
        }

        private Vector2 GetWheelPosition(int offset) {
            return direction switch {
                WheelDirection.Vertical => new Vector2(960, 540 + offset * 200),
                WheelDirection.Horizontal => new Vector2(960 + offset * 300, 540),
                WheelDirection.Curved => new Vector2(960 + offset * 250, 540 + (float)Math.Sin(offset * 0.5f) * 100),
                _ => new Vector2(960, 540)
            };
        }

        private float GetScale(int offset) {
            return 1f - MathF.Abs(offset) * 0.1f;
        }
    }
}
