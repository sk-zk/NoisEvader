using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Styles;

namespace NoisEvader.UI
{
    public partial class Settings : Window, IScreen
    {
        private GraphicsDeviceManager graphics;

        private List<DisplayMode> modes;

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Settings(GraphicsDeviceManager graphics)
        {
            this.graphics = graphics;

            BuildUI();

            TitleGrid.Visible = false;

            Back.Click += Back_Click;
            Save.Click += Save_Click;
            MusicVolume.ValueChanged += MusicVolume_ValueChanged;
        }

        public void Show()
        {
            LoadSettingsIntoUi();
            ShowModal(NoisEvader.Desktop);
        }

        private void Save_Click(object sender, EventArgs e)
        {
            SaveSettings();
            Close();
        }

        private void Back_Click(object sender, EventArgs e)
        {
            AudioPlayer.Instance.Volume = Config.GameSettings.Volume;
            Close();
        }

        private void LoadSettingsIntoUi()
        {
            PopulateResolution();

            WindowMode.Items.Clear();
            foreach (WindowType type in Enum.GetValues(typeof(WindowType)))
                WindowMode.Items.Add(new ListItem(type.ToString()));

            ScaleMode.Items.Clear();
            foreach (ScaleMode mode in Enum.GetValues(typeof(ScaleMode)))
                ScaleMode.Items.Add(new ListItem(mode.ToString()));

            var s = Config.GameSettings;
            WindowMode.SelectedIndex = (int)s.WindowType;
            ScaleMode.SelectedIndex = (int)s.ScaleMode;
            Vsync.IsPressed = s.Vsync;
            ShowFps.IsPressed = s.ShowFps;
            DrawOuterRings.IsPressed = s.DrawOuterRings;
            DrawHitbox.IsPressed = s.DrawPlayerHitbox;
            ScreenShake.IsPressed = s.ScreenShakeOnHit;
            DrawParticles.IsPressed = s.DrawParticles;
            DrawGlow.IsPressed = s.DrawGlow;
            StretchyPlayer.IsPressed = s.StretchyPlayer;
            MusicVolume.Value = s.Volume * 100;
            MusicVolumeLabel.Text = MusicVolume.Value.ToString("0.0");
        }

        private void PopulateResolution()
        {
            Resolution.Items.Clear();
            modes = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes.ToList();
            foreach (var mode in modes)
                Resolution.Items.Add(new ListItem($"{mode.Width}x{mode.Height}"));

            var bounds = graphics.GraphicsDevice.PresentationParameters.Bounds;
            var current = modes.FindIndex(x => x.Width == bounds.Width && x.Height == bounds.Height);
            Resolution.SelectedIndex = current;
        }

        private void SaveSettings()
        {
            logger.Info("User changed settings");
            var s = Config.GameSettings;
            s.WindowType = (WindowType)WindowMode.SelectedIndex;
            s.ScaleMode = (ScaleMode)ScaleMode.SelectedIndex;
            s.Vsync = Vsync.IsPressed;
            s.ShowFps = ShowFps.IsPressed;
            s.DrawOuterRings = DrawOuterRings.IsPressed;
            s.DrawPlayerHitbox = DrawHitbox.IsPressed;
            s.ScreenShakeOnHit = ScreenShake.IsPressed;
            s.DrawParticles = DrawParticles.IsPressed;
            s.DrawGlow = DrawGlow.IsPressed;
            s.StretchyPlayer = StretchyPlayer.IsPressed;
            s.Volume = MusicVolume.Value / 100;

            var dialog = ApplyGraphicsChanges();

            Config.SaveSettings();

            NoisEvader.Desktop.UpdateLayout();

            if (dialog != null)
                dialog.ShowModal(NoisEvader.Desktop);
        }

        /// <summary>
        /// Applies graphics settings.
        /// </summary>
        /// <returns>Whether or not the method creared a message box.</returns>
        private MessageBox ApplyGraphicsChanges()
        {
            logger.Info("Applying graphics settings");
            var currentlyFullscreen = graphics.IsFullScreen && graphics.HardwareModeSwitch;
            if (Resolution.SelectedIndex.HasValue)
            {
                var width = modes[Resolution.SelectedIndex.Value].Width;
                var height = modes[Resolution.SelectedIndex.Value].Height;
                Config.GameSettings.ScreenWidth = width;
                Config.GameSettings.ScreenHeight = height;
            }

            if (currentlyFullscreen)
            {
                return new MessageBox("Please restart the game to apply settings.");
            }
            else
            {
                graphics.PreferredBackBufferWidth = Config.GameSettings.ScreenWidth;
                graphics.PreferredBackBufferHeight = Config.GameSettings.ScreenHeight;
                graphics.HardwareModeSwitch = Config.GameSettings.WindowType == WindowType.Fullscreen;
                graphics.IsFullScreen = Config.GameSettings.WindowType != WindowType.Windowed;
                graphics.ApplyChanges();
                return null;
            }
        }

        private void MusicVolume_ValueChanged(object sender, Myra.Utility.ValueChangedEventArgs<float> e)
        {
            MusicVolumeLabel.Text = e.NewValue.ToString("0.0");
            AudioPlayer.Instance.Volume = e.NewValue / 100;
        }
    }
}