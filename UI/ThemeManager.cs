using System.Numerics;

namespace Arcadia.UI {
    public static class ThemeManager {
        public static void DrawBoxart(string path, Vector2 position, float scale) {
            // Placeholder: integrate with your rendering backend (Direct3D/OpenGL)
            Renderer.DrawImage(path, position, scale);
        }

        public static void DrawLogo(string path, Vector2 position, float scale) {
            Renderer.DrawImage(path, position, scale);
        }

        public static void ApplyShader(string shaderName) {
            Renderer.SetShader(shaderName);
        }
    }
}
