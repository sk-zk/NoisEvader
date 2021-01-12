using Microsoft.Xna.Framework;
using Myra.Graphics2D.UI;
using System;

namespace NoisEvader.UI
{
    public partial class ArenaPause : IScreen
    {
        public event EventHandler ContinuePressed, ExitPressed,
            RestartPressed;

        public ArenaPause()
        {
            BuildUI();

            Restart.Click += Restart_Click;
            Continue.Click += Continue_Click;
            Exit.Click += Exit_Click;
        }

        private void Restart_Click(object sender, EventArgs e)
        {
            RestartPressed?.Invoke(this, null);
        }

        private void Continue_Click(object sender, EventArgs e)
        {
            ContinuePressed?.Invoke(this, null);
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            ExitPressed.Invoke(this, null);
        }

        public void Show()
        {
            NoisEvader.Desktop.Root = this;
            NoisEvader.Desktop.Root.Visible = true;
        }
    }
}