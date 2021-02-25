using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Styles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNAssets;
using System.IO;
using NoisEvader.Replays;
using FontStashSharp;

namespace NoisEvader
{
    public partial class NoisEvader
    {
        private const string GameVersionString = "prealpha0";
        public static Desktop Desktop;
        private UI.MainMenu mainMenu;
        private UI.LevelSelect levelSelectScreen;
        private UI.ArenaPause arenaPauseScreen;
        private bool settingsOpen;

        private void InitGui()
        {
            logger.Info("Initializing UI");
            MyraEnvironment.Game = this;

            Desktop = new Desktop();

            var assetManager = new AssetManager(GraphicsDevice,
               new ResourceAssetResolver(typeof(NoisEvader).Assembly, "Resources"));
            Stylesheet.Current = assetManager.Load<Stylesheet>("default_ui_skin.xml");

            // set fonts at runtime rather than through the stylesheet
            // to allow for fallback fonts
            var orkney24 = Fonts.OrkneyWithFallback.GetFont(24);
            var orkney32 = Fonts.OrkneyWithFallback.GetFont(32);
            Stylesheet.Current.LabelStyle.Font = orkney24;
            Stylesheet.Current.ButtonStyle.LabelStyle.Font = orkney24;
            Stylesheet.Current.ListBoxStyle.ListItemStyle.LabelStyle.Font = orkney24;
            Stylesheet.Current.ComboBoxStyle.ListBoxStyle.ListItemStyle.LabelStyle.Font = orkney24;
            Stylesheet.Current.ComboBoxStyle.LabelStyle.Font = orkney24;
            Stylesheet.Current.CheckBoxStyle.LabelStyle.Font = orkney24;
            Stylesheet.Current.WindowStyle.TitleStyle.Font = orkney24;
            Stylesheet.Current.ButtonStyles["MainMenuBtn"].LabelStyle.Font = orkney32;
            Stylesheet.Current.ButtonStyles["PauseScreenBtn"].LabelStyle.Font = orkney32;
            Stylesheet.Current.LabelStyles["Header"].Font = orkney32;

            var props = Stylesheet.Current.GetType().GetProperties();

            mainMenu = new UI.MainMenu();
            mainMenu.Version.Text = GameVersionString;
            mainMenu.PlayPressed += MainMenu_PlayPressed;
            mainMenu.SettingsPressed += MainMenu_SettingsPressed;
            mainMenu.ExitPressed += MainMenu_ExitPressed;

            levelSelectScreen = new UI.LevelSelect();
            levelSelectScreen.BackPressed += LevelSelectScreen_BackButtonPressed;
            levelSelectScreen.LevelSelected += LevelSelectScreen_LevelSelected;
            levelSelectScreen.ReplaySelected += LevelSelectScreen_ReplaySelected;
            Window.ClientSizeChanged += Window_ClientSizeChanged;

            arenaPauseScreen = new UI.ArenaPause();
            arenaPauseScreen.ContinuePressed += ArenaPauseScreen_ContinuePressed;
            arenaPauseScreen.ExitPressed += ArenaPauseScreen_ExitPressed;
            arenaPauseScreen.RestartPressed += ArenaPauseScreen_RestartPressed;
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            UpdateScreenBoundsProperty();
            menuScene.OnWindowResized();
            InitScreenshotTarget();

            Arena?.OnWindowResized();

            // [HACK] Fixes labels getting 0 width after resizing the window
            levelSelectScreen.OnWindowResized();
        }

        private void LevelSelectScreen_ReplaySelected(object sender, CachedLevelData cld, 
            string replayId)
        {
            var level = new SoundodgerLevel(cld.XmlPath);
            LoadReplayAndStart(level, replayId, cld.Settings);
        }

        public void LoadReplayAndStart(SoundodgerLevel level, string replayId, LevelSettings settings)
        {
            logger.Info("Replay selected: {replayId} for {artist} - {title} by {designer} ({hash})",
                replayId, level.Info.Artist, level.Info.Title, level.Info.Designer, level.XmlHash);

            Arena = new ReplayArena(inputHelper);
            InitArena(Arena);
            Desktop.Root.Visible = false;
            IsMouseVisible = false;
            SetVsync(Config.GameSettings.Vsync);
            try
            {
                var replay = Replay.Open(Path.Combine(Replay.ReplaysDir, replayId));
                (Arena as ReplayArena).LoadReplayAndStart(level, replay, settings);
                gameState = GameState.PlayingLevel;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Replay failed to load");
                SwitchToLevelSelectScreen();
                ShowErrorMessage("The replay failed to load.\nCheck the game log for details.");
            }
        }

        public void SwitchToArenaAndStart(SoundodgerLevel level, Mod mods, LevelSettings settings)
        {
            logger.Info("Level selected: {artist} - {title} by {designer} ({hash})",
                level.Info.Artist, level.Info.Title, level.Info.Designer, level.XmlHash);
            Arena = new Arena(inputHelper);
            InitArena(Arena);
            Desktop.Root.Visible = false;
            IsMouseVisible = false;
            SetVsync(Config.GameSettings.Vsync);
            try
            {
                Arena.LoadLevelAndStart(level, mods, settings);
                gameState = GameState.PlayingLevel;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Level failed to load");
#if DEBUG
                throw;
#else
                SwitchToLevelSelectScreen();
                ShowErrorMessage("The level failed to load.\nCheck the game log for details.");
#endif
            }
        }

        private static void ShowErrorMessage(string message)
        {
            var dlg = new UI.MessageBox(message);
            dlg.ShowModal(Desktop);
        }

        private void SwitchToLevelSelectScreen()
        {
            gameState = GameState.Menu;
            SetVsync(vsyncInMenus);
            Desktop.Root.Visible = true;
            IsMouseVisible = true;
            levelSelectScreen.ReloadSongInfo();
            levelSelectScreen.Show();
        }

        private void UnpauseArena()
        {
            Desktop.Root = null;
            IsMouseVisible = false;
        }

        private void SwitchToMainMenu()
        {
            gameState = GameState.Menu;
            SetVsync(vsyncInMenus);
            if (Desktop != null && Desktop.Root != null)
                Desktop.Root.Visible = true;
            IsMouseVisible = true;
            mainMenu.Show();
        }

        private void MainMenu_SettingsPressed(object sender, EventArgs e)
        {
            ShowSettings();
        }

        private void ShowSettings()
        {
            if (settingsOpen)
                return;

            settingsOpen = true;
            var settingsScreen = new UI.Settings(graphics);
            settingsScreen.Closed += (_, _) => settingsOpen = false;
            settingsScreen.CenterOnDesktop();
            settingsScreen.Show();
        }

        private void MainMenu_PlayPressed(object sender, EventArgs e)
        {
            SwitchToLevelSelectScreen();
        }

        private void MainMenu_ExitPressed(object sender, EventArgs e)
        {
            Exit();
        }

        private void LevelSelectScreen_BackButtonPressed(object sender, EventArgs e)
        {
            SwitchToMainMenu();
        }

        private void LevelSelectScreen_LevelSelected(object sender, CachedLevelData cld, Mod mods)
        {
            var level = new SoundodgerLevel(cld.XmlPath);
            SwitchToArenaAndStart(level, mods, cld.Settings);
        }

        private void Arena_ExitedToMenu(object sender, Score score)
        {
            gameState = GameState.Menu;
            Desktop.Root.Visible = true;
            if (score != null)
                levelSelectScreen.UpdateCachedDataOfSelected(score);
            SwitchToLevelSelectScreen();
        }

        private void Arena_PausePressed(object sender, EventArgs e)
        {
            arenaPauseScreen.Show();
            IsMouseVisible = true;
        }

        private void Arena_ArenaUnpaused(object sender, EventArgs e)
        {
            UnpauseArena();
        }

        private void ArenaPauseScreen_ContinuePressed(object sender, EventArgs e)
        {
            Arena.Paused = false;
            IsMouseVisible = false;
        }

        private void ArenaPauseScreen_ExitPressed(object sender, EventArgs e)
        {
            logger.Info("Player quit level");
            Arena.ExitToMenu();
            Desktop.Root.Visible = true;
            IsMouseVisible = true;
        }

        private void ArenaPauseScreen_RestartPressed(object sender, EventArgs e)
        {
            RestartCurrentLevel();
        }

        private void RestartCurrentLevel()
        {
            audioPlayer.Stop();
            var level = new SoundodgerLevel(Arena.Level.XmlPath);
            SwitchToArenaAndStart(level, Arena.ActiveMod, Arena.LevelSettings);
        }
    }
}
