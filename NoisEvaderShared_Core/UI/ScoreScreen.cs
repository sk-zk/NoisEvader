using Microsoft.Xna.Framework;
using Myra.Graphics2D.UI;
using System;

namespace NoisEvader.UI
{
    public partial class ScoreScreen : IScreen
    {
        public event EventHandler BackPressed;

        public ScoreScreen()
        {
            BuildUI();

            Back.Click += (sender, e) => BackPressed?.Invoke(sender, e);
        }

        public void Show() => NoisEvader.Desktop.Root = this;
    }
}
