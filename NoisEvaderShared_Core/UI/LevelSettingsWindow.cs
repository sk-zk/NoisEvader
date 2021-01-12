namespace NoisEvader.UI
{
    public partial class LevelSettingsWindow
    {
        private LevelSettings settings;

        public LevelSettings Settings 
        { 
            get => settings;
            set
            { 
                settings = value;
                ThirtyTicks.IsPressed = settings.ThirtyTicks;
                NaiveWarp.IsPressed = settings.NaiveWarp;
                InvertColors.IsPressed = settings.InvertColors; 
            }
        }

        public LevelSettingsWindow()
        {
            BuildUI();

            ThirtyTicks.Click += ThirtyTicks_Click;
            NaiveWarp.Click += NaiveWarp_Click;
            InvertColors.Click += InvertColors_Click;
        }

        private void NaiveWarp_Click(object sender, System.EventArgs e)
        {
            settings.NaiveWarp = NaiveWarp.IsPressed;
        }

        private void ThirtyTicks_Click(object sender, System.EventArgs e)
        {
            settings.ThirtyTicks = ThirtyTicks.IsPressed;
        }

        private void InvertColors_Click(object sender, System.EventArgs e)
        {
            settings.InvertColors = InvertColors.IsPressed;
        }
    }
}