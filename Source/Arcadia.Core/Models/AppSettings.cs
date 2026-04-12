namespace Arcadia.Core.Models
{
    public class AppSettings
    {
        public string WheelOrientation { get; set; } = "angled";
        public float TiltAngle { get; set; } = -0.5f;
        public float WheelRadius { get; set; } = 500f;
        public float ItemSpacing { get; set; } = 0.25f;
        public float LinearSpacing { get; set; } = 150f;
        public float WheelXOffset { get; set; } = 200f;
        public float WheelYOffset { get; set; } = 500f;
        public float LogoWidth { get; set; } = 300f;
        public float LogoHeight { get; set; } = 150f;
        public string AccentColor { get; set; } = "#FFFFFF";
        public bool IsFullscreen { get; set; } = false;
        public string Theme { get; set; } = "Default";
        public double MasterVolume { get; set; } = 1.0;
        public bool AttractModeEnabled { get; set; } = true;
        public int AttractModeDelay { get; set; } = 30; // seconds
        public string Language { get; set; } = "en-US";
    }
}