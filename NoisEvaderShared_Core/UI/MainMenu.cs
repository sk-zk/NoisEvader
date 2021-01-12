using System;
using Microsoft.Xna.Framework;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.TextureAtlases;

namespace NoisEvader.UI
{
    public partial class MainMenu : IScreen
    {
        public event EventHandler ExitPressed;
        public event EventHandler PlayPressed;
        public event EventHandler SettingsPressed;

        public static TextureRegion LogoTexture { get; set; }

        public MainMenu()
        {
            BuildUI();

            Logo.Renderable = LogoTexture;

            Play.Click += Play_Click;
            Settings.Click += Settings_Click;
            Exit.Click += Exit_Click;
        }

        private void Play_Click(object sender, EventArgs e)
            => PlayPressed?.Invoke(this, null);

        private void Settings_Click(object sender, EventArgs e)
            => SettingsPressed?.Invoke(this, null);

        private void Exit_Click(object sender, EventArgs e)
            => ExitPressed?.Invoke(this, null);

        public void Show() => NoisEvader.Desktop.Root = this;
    }
}