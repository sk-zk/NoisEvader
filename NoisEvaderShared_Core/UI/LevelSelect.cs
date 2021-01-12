using Microsoft.Xna.Framework;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Threading.Tasks;

namespace NoisEvader.UI
{
    public partial class LevelSelect : IScreen
    {
        public event EventHandler BackPressed;
        public delegate void LevelSelectedHandler(object sender, CachedLevelData level,  Mod mods);
        public event LevelSelectedHandler LevelSelected;
        public delegate void ReplaySelectedHandler(object sender, CachedLevelData level, string replayName);
        public event ReplaySelectedHandler ReplaySelected;

        private LevelBox levelBox;

        private List<Score> scores;

        private const int FadeInTime = 900;
        private const int FadeOutTime = 500;

        private ModWindow modWindow = new ModWindow();
        private LevelSettingsWindow levelSettingsWindow = new LevelSettingsWindow();

        private Mod activeMod;

        public static TextureRegion HeartIcon { get; set; }
        public static TextureRegion HeartOutlineIcon { get; set; }

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public LevelSelect()
        {
            BuildUI();

            levelBox = new LevelBox();
            LevelScrollViewer.Content = levelBox;
            levelBox.LevelSelected += LevelBox_LevelSelected;

            Title.Font = Fonts.OrkneyWithFallback.GetFont(40);
            Artist.Font = Fonts.OrkneyWithFallback.GetFont(32);
            Difficulty.Font = Fonts.DejaVuSans.GetFont(24);
            Length.Font = Fonts.Orkney.GetFont(18);
            Creator.Font = Fonts.Orkney.GetFont(18);

            LevelInfo.Visible = false;

            UIUtil.SetHeart(HeartImg, 28);

            activeMod = Mod.None;

            modWindow.MinWidth = 170;
            levelSettingsWindow.MinWidth = 170;

            Back.Click += Back_Click;
            Play.Click += Play_Click;
            Mods.Click += Mods_Click;
            LevelSettings.Click += LevelSettings_Click;
            HideFolders.Click += HideFolders_Click;
            modWindow.Ok.Click += ModsOk_Click;
            levelSettingsWindow.Ok.Click += LevelSettingsOk_Click;

            levelBox.LevelFolder = GetLevelFolder();
            PopulateSortBox();
            SortMode.SelectedIndexChanged += SortMode_SelectedIndexChanged;
        }

        public void Update()
        {
            levelBox.Update();
        }

        private void LevelSettingsOk_Click(object sender, EventArgs e)
        {
            levelSettingsWindow.Close();
            var settings = levelSettingsWindow.Settings;
            levelBox.SelectedLevel.Settings = settings;
            Database.UpdateLevelSettings(levelBox.SelectedLevel.XmlHash, settings);
            Mod.ApplyLevelSettings(ref activeMod, settings);
        }

        private void LevelSettings_Click(object sender, EventArgs e)
        {
            levelSettingsWindow.Settings = levelBox.SelectedLevel.Settings;
            levelSettingsWindow.TitleGrid.Visible = false;
            levelSettingsWindow.ShowModal(NoisEvader.Desktop);
        }

        private void HideFolders_Click(object sender, EventArgs e)
        {
            levelBox.HideFolders = HideFolders.IsPressed;
        }

        private void ModsOk_Click(object sender, EventArgs e)
        {
            modWindow.Close();
            activeMod = modWindow.Mod;
        }

        private void Mods_Click(object sender, EventArgs e)
        {
            modWindow.Mod = activeMod;
            modWindow.TitleGrid.Visible = false;
            modWindow.ShowModal(NoisEvader.Desktop);
        }

        private void LevelBox_LevelSelected(object sender, CachedLevelData e)
        {
            Mod.ApplyLevelSettings(ref activeMod, e.Settings);
            SetDisplayedLevel(e);
        }

        private void SetDisplayedLevel(CachedLevelData level)
        {
            Play.Enabled = true;
            StartSongPreview(level);
            ShowLevelInfo(level);
        }

        private void SortMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            levelBox.Sort = (SortType)SortMode.SelectedIndex;
        }

        private void Back_Click(object sender, EventArgs e)
            => BackPressed?.Invoke(this, null);

        private void Play_Click(object sender, EventArgs e)
        {
            LevelSelected?.Invoke(this, levelBox.SelectedLevel, activeMod);
        }

        private void StartSongPreview(CachedLevelData level)
        {
            var player = AudioPlayer.Instance;
            var audioFile = level.Mp3Path;

            if (player.CurrentFile == audioFile)
            {
                RestartSongPreview(level);
                return;
            }

            if (File.Exists(level.Mp3Path))
            {
                Task.Run(() =>
                {
                    lock (player)
                    {
                        player.Stop();
                        player.Load(level.Mp3Path, false, false);
                        var previewPoint = (int)(player.TotalTime.TotalMilliseconds
                            * (level.Info.AudioPreviewPoint / 100f));
                        player.JumpTo(previewPoint);
                        player.BeginFadeIn(FadeInTime);
                        player.Play();
                    }
                });
            }
            else
            {
                //errorMsg.Text = "Audio file is missing.";
                Play.Enabled = false;
            }
        }

        private void RestartSongPreview(CachedLevelData level)
        {
            var player = AudioPlayer.Instance;
            lock (player)
            {
                // Preview already playing or user abandoned level early
                if (player.PositionPercent < 1)
                {
                    if (!player.IsPlaying)
                    {
                        player.PlaybackSpeed = 1;
                        player.BeginFadeIn(FadeInTime);
                        try
                        {
                            player.Play();
                        } 
                        catch (ObjectDisposedException odex)
                        {
                            // TODO: prevent this exception
                            // which happens when exiting the level before audio finished loading
                            logger.Warn(odex);
                        }
                    }
                }
                // returned to level select after level ended
                else
                {
                    player.PlaybackSpeed = 1;
                    var previewPoint = (int)(player.TotalTime.TotalMilliseconds
                            * (level.Info.AudioPreviewPoint / 100f));
                    player.JumpTo(previewPoint);
                    player.BeginFadeIn(FadeInTime);
                    player.Play();
                }
            }
        }

        private void ShowLevelInfo(CachedLevelData level)
        {
            LevelInfo.Visible = true;

            Task.Run(() =>
            {
                lock (Length)
                {
                    if (level.AudioDuration is null)
                    {
                        Length.Text = "0:00 – ";
                        level.LoadAudioDuration();
                    }
                    Length.Text = UIUtil.GetDurationString(level.AudioDuration ?? TimeSpan.Zero) + " – ";
                }
            });

            Title.Text = level.Info.Title;
            Artist.Text = level.Info.Artist;
            Subtitle.Text = UIUtil.GetSubtitle(level.Info);
            Difficulty.Text = UIUtil.GetDifficultyString(level.Info.Difficulty);
            Creator.Text = level.Info.Designer;
            HeartImg.Visible = level.Info.HasHeart;

            ShowLocalScores(level);

            if (scores is null || scores.Count == 0)
            {
                HeartImg.Renderable = HeartOutlineIcon;
            }
            else
            {
                HeartImg.Renderable = scores.Any(x => x.HeartGotten)
                    ? HeartIcon 
                    : HeartOutlineIcon;
            }

            Playcount.Text = "Plays: " + level.Playcount.ToString();
        }

        internal void PlayerGotHeartOnSelected()
        {
            levelBox.SelectedLevel.HeartGotten = true;
            levelBox.UpdateSelected();
        }

        private void ShowLocalScores(CachedLevelData level)
        {
            ScoresContainer.Widgets.Clear();
            scores = Database.GetScores(level.XmlHash);
            if (scores is null || scores.Count == 0)
                return;

            scores = scores.OrderByDescending(x => x.Percent * x.Mod.Multiplier)
                .ThenBy(x => x.HeartGotten)
                .ToList();

            for (int i = 0; i < scores.Count; i++)
            {
                var panel = new ScorePanel(scores[i], level.Info.HasHeart);
                var replayFile = scores[i].ReplayFile;
                panel.TouchDown += (_, __) =>
                {
                    if (!string.IsNullOrEmpty(replayFile))
                        ReplaySelected?.Invoke(panel, levelBox.SelectedLevel, replayFile);
                };
                panel.GridRow = i;
                ScoresContainer.Widgets.Add(panel);
            }
        }

        private void PopulateSortBox()
        {
            SortMode.Items.Clear();
            foreach (SortType type in Enum.GetValues(typeof(SortType)))
            {
                var item = new ListItem(type.ToString());
                SortMode.Items.Add(item);
            }
            SortMode.SelectedIndex = (int)levelBox.Sort;
        }

        public static string GetLevelFolder()
        {
            var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var assumedLocation = Path.Combine(documentsFolder, "soundodger");
            if (Directory.Exists(assumedLocation))
                return assumedLocation;
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        public void OnWindowResized()
        {
            // [HACK] Fixes labels getting 0 width after resizing the window
            levelBox.WindowResized();
        }

        public void ReloadSongInfo()
        {
            if (levelBox.SelectedLevel != null)
            {
                SetDisplayedLevel(levelBox.SelectedLevel);
            }
        }

        public void Show()
        {
            NoisEvader.Desktop.Root = this;
            Visible = true;
            if (!levelBox.FirstLoadDone)
                levelBox.FirstLoad();
        }

    }
}