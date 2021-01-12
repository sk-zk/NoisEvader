namespace NoisEvader
{
    public class GameSettings
    {
        public int ScreenWidth { get; set; } = 1280;
        public int ScreenHeight { get; set; } = 720;
        public WindowType WindowType { get; set; } = WindowType.Windowed;
        public bool Vsync { get; set; } = false;
        public bool ShowFps { get; set; } = false;
        public ScaleMode ScaleMode { get; set; } = ScaleMode.Fit;

        public bool DrawOuterRings { get; set; } = true;
        public bool ScreenShakeOnHit { get; set; } = true;
        public bool DrawPlayerHitbox { get; set; } = false;
        public bool DrawGlow { get; set; } = true;
        public bool DrawParticles { get; set; } = true;
        public bool StretchyPlayer { get; set; } = true;

        public float Volume { get; set; } = 0.95f;
    }

}