using LilyPath;
using LilyPath.Pens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoisEvader.Replays;

namespace NoisEvader
{
    public class Arena
    {
        public const float ArenaRadius = 200;

        protected SoundodgerLevel level;
        public SoundodgerLevel Level => level;

        public Mod ActiveMod;
        public LevelSettings LevelSettings;

        public ArenaState State { get; protected set; } = ArenaState.Idle;

        protected ArenaCircle arena;
        protected Player player;
        protected ArenaSpawners spawners;
        protected BulletManager bulletMgr;

        private OuterRings outerRings;
        protected CircleParticleSystem particles = new CircleParticleSystem();

        protected TimeWarps timeWarp;
        protected double currentTimeWarp;
        protected TimeWarps spinRate;
        protected double currentSpinRate;
        protected PiecewiseFunction combinedSpinWarpFunction;

        private AudioPlayer Audio => AudioPlayer.Instance;
        protected double songPosition;
        protected double prevSongPosition;
        protected double songElapsed;

        protected ArenaCamera camera;
        private ScreenShake screenShake;

        protected InputHelper inputHelper;

        private RenderTarget2D flashlightTarget;
        private FlashlightVignette flashlight;

        private Vector2 ScreenCenter => NoisEvader.ScreenCenter;

        public Matrix ScreenTransform =>
            camera.Transform * Matrix.CreateTranslation(screenShake.Offset);

        public float BaseGameSpeed { get; set; } = NormalGameSpeed;
        private const float SlomoGameSpeed = 0.5f;
        private const float NormalGameSpeed = 1f;

        private float GameSpeed
        {
            get
            {
                if (slomo)
                    return BaseGameSpeed * SlomoGameSpeed;
                return BaseGameSpeed;
            }
        }

        private bool slomo;
        private double slomoStart;
        private double totalSlomoTime;

        public Color BackgroundColor
        {
            get
            {
                if (level is null)
                    return Color.Black;

                return slomo ?
                    level.Info.Colors.SlomoBackground
                    : level.Info.Colors.Background;
            }
        }

        private MouseState mouseStateBeforePause;
        private bool paused;
        public bool Paused
        {
            get => paused;
            set
            {
                paused = value;
                if (paused)
                    Pause();
                else
                    Unpause();
            }
        }

        /// <summary>
        /// For how long the level has to be played to increase the playcount of the level.
        /// </summary>
        private const float PlayThreshold = 10000;

        private static UI.ScoreScreen scoreScreen;
        private SimpleText skipIntroText;
        private SimpleText debugText;

        // skip intro stuff
        private float firstShotTime;
        private int acclimation = 800;
        private int minIntroDuration = 4000;

        private ReplayRecorder replayRecorder;

        private Score score = null;
        public event EventHandler<Score> ExitedToMenu;
        public event EventHandler ArenaPaused;
        public event EventHandler ArenaUnpaused;
        public event EventHandler<float?> TickLimitChanged;

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Arena(InputHelper inputHelper)
        {
            this.inputHelper = inputHelper;

            arena = new ArenaCircle();

            // create camera stuff
            camera = new ArenaCamera();
            screenShake = new ScreenShake();

            // keeping this stuff disabled until I can make UpdateGameObjects
            // run fast enough for this to make sense.
            /* posScreen = new UI.LevelPositionScreen();
            posScreen.Position.ValueChangedByUser += PosScreenValueChanged; */
            scoreScreen = new UI.ScoreScreen();
            scoreScreen.BackPressed += (_, __) => ExitToMenu();

            debugText = new SimpleText()
            {
                Font = Fonts.Content.DebugFont,
                Position = new Vector2(10, 10),
                Color = Color.Gray,
                Visible = false,
            };
        }

        private void PosScreenValueChanged(object sender, Myra.Utility.ValueChangedEventArgs<float> e)
        {
            if (e.NewValue < Audio.Position)
                    ResetEverything();
            SkipTo(e.NewValue * 1000);
        }

        private void ResetEverything()
        {
            player.Position = Vector2.Zero;
            spawners.Reset();
            bulletMgr.Reset();
            outerRings.Clear();
            songPosition = 0;
            prevSongPosition = 0;
            songElapsed = 0;
        }

        public void LoadLevelAndStart(SoundodgerLevel level, Mod mod, LevelSettings settings)
        {
            replayRecorder = new ReplayRecorder();
            replayRecorder.Replay.Mod = mod;
            player = new HumanPlayer();
            InitAndStart(level, mod, settings);
        }

        protected void InitAndStart(SoundodgerLevel level, Mod mod, LevelSettings settings)
        {
            logger.Info("Loading level");
            State = ArenaState.Loading;
            this.level = level;

            // init mods
            ActiveMod = mod;
            BaseGameSpeed = mod.GameSpeed;

            if (ActiveMod.TickRate.HasValue)
                TickLimitChanged?.Invoke(this, mod.TickRate * mod.GameSpeed);

            // init game elements
            arena.SetScale(ActiveMod.Flags.HasFlag(ModFlags.Live));

            LevelSettings = settings;
            if (settings.InvertColors)
            {
                level.Info.Colors.Invert();
                level.Info.ArenaGlowEnabled = !level.Info.ArenaGlowEnabled;
            }

            arena.SetColors(level.Info.Colors, BackgroundColor);

            outerRings = new OuterRings(level.Info.Colors.OuterRings, arena.Circle);

            player.Position = Vector2.Zero;
            player.BorderColor = level.Info.Colors.Outline;

            timeWarp = new TimeWarps(level.Script.TimeWarpNodes);
            spinRate = new TimeWarps(level.Script.SpinRateNodes);
            combinedSpinWarpFunction = new PiecewiseFunction(level.Script.TimeWarpNodes)
                * new PiecewiseFunction(level.Script.SpinRateNodes);

            InitSpawners();

            bulletMgr = new BulletManager(arena, level, spawners.Spawners, mod, particles);

            if (mod.Flags.HasFlag(ModFlags.Flashlight))
            {
                flashlight = new FlashlightVignette();
                flashlightTarget = new RenderTarget2D(
                    NoisEvader.Gd, NoisEvader.ScreenBounds.Width, NoisEvader.ScreenBounds.Height);
            }

            Mouse.SetPosition((int)ScreenCenter.X, (int)ScreenCenter.Y);
            player.Center = arena.Circle.Center;

            InitSkipIntro();

            if (player is HumanPlayer p)
            {
                replayRecorder.Replay.XmlHash = level.XmlHash;
                replayRecorder.Replay.Timestamp = DateTime.UtcNow;
                replayRecorder.Replay.BurstSeed = BurstShot.Seed;
            }

            Audio.PlaybackSpeed = GameSpeed;
            Task.Run(() =>
            {
                Audio.Load(level.AudioPath);
                audioLoaded = true;
            });
        }

        protected virtual void InitSpawners()
        {
            if (ActiveMod.Flags.HasFlag(ModFlags.Live))
                spawners = new LiveSpawners(arena, combinedSpinWarpFunction);
            else
                spawners = new ArenaSpawners(arena, combinedSpinWarpFunction);
            spawners.CreateSpawners(level);
            spawners.CreateIndicatorList(level);
        }

        private bool audioLoaded;

        private void InitSkipIntro()
        {
            firstShotTime = (float)GetFirstShot().Time;

            var screenSpaceRadius = camera.MainScale * ArenaRadius;
            var yPos = ScreenCenter.Y + (screenSpaceRadius * 0.6f);

            skipIntroText = new SimpleText()
            {
                Font = Fonts.Content.Orkney13,
                Color = level.Info.Colors.Background.Invert(),
                Text = "press space to skip intro",
                Position = new Vector2(ScreenCenter.X, yPos),
                XOrigin = XOrigin.Center,
                YOrigin = YOrigin.Center,
                Visible = false,
            };
        }

        private void Pause()
        {
            if (State == ArenaState.Playing)
                Audio.Pause();

            mouseStateBeforePause = Mouse.GetState();
            ArenaPaused?.Invoke(this, null);
        }

        private void Unpause()
        {
            ArenaUnpaused?.Invoke(this, null);
            Mouse.SetPosition(mouseStateBeforePause.X, mouseStateBeforePause.Y);

            if (State == ArenaState.Playing)
                Audio.Resume();
        }

        public void Update(GameTime gameTime)
        {
            if (State == ArenaState.Idle)
                return;

            arena.GlowEnabled = slomo || level.Info.ArenaGlowEnabled;

            if (inputHelper.KeyPressed(Keys.F3))
                debugText.Visible = !debugText.Visible;

            if (Paused)
            {
                UpdatePaused();
                return;
            }

            switch (State)
            {
                case ArenaState.Loading:
                    UpdateLoading(gameTime);
                    break;
                case ArenaState.Playing:
                    UpdatePlaying(gameTime);
                    break;
                case ArenaState.ScoreScreen:
                    UpdateScoreScreen(gameTime);
                    break;
            }
        }

        private void UpdatePlaying(GameTime gameTime)
        {
            #if DEBUG
            if (inputHelper.KeyPressed(Keys.H))
            {
                player.Invincibility = (player.Invincibility != InvincibilityType.Eternal) ?
                    InvincibilityType.Eternal : InvincibilityType.None;
            }
            #endif

            UpdateSlomo();
            UpdateAudio();
            UpdateSpinAndWarp();

            camera.Update(gameTime);

            if (songElapsed != 0 && !ActiveMod.TickRate.HasValue)
            {
                // use audio-based timekeeping while the song is playing
                // (unless we're in 30 tick mode because it would cause jitters)
                gameTime = new GameTime(
                    gameTime.TotalGameTime, TimeSpan.FromMilliseconds(songElapsed));
            }

            var levelTime = new LevelTime()
            {
                GameTime = gameTime,
                SongPosition = songPosition,
                GameSpeed = GameSpeed,
                TimeWarp = currentTimeWarp,
                SpinRate = currentSpinRate,
            };

            // keeping this stuff disabled until I can make UpdateGameObjects
            // run fast enough for this to make sense.
            /*
            if (posScreen != null)
                posScreen.Position.Value = songPosition / 1000;
            */

            UpdateSkipIntro();
            UpdateGameObjects(levelTime);

            replayRecorder?.Update((float)gameTime.ElapsedGameTime.TotalMilliseconds);

            if (inputHelper.KeyPressed(Keys.Escape))
            {
                Paused = true;
                return;
            }

            if (Audio.PositionPercent > 0.99 && bulletMgr.Bullets.Count == 0)
            {
                EndLevel();
            }
        }

        private void UpdateAudio()
        {
            Audio.Update(GameSpeed);
            prevSongPosition = songPosition;
            songPosition = Audio.Position;
            songElapsed = songPosition - prevSongPosition;
        }

        private double eswTimeSinceLastWarpUpdate;
        private void UpdateSpinAndWarp()
        {
            if (ActiveMod.Flags.HasFlag(ModFlags.NaiveWarp))
            {
                const float warpTickRate = 1000f / 30;
                eswTimeSinceLastWarpUpdate += songElapsed;

                if (eswTimeSinceLastWarpUpdate < warpTickRate)
                    return;

                eswTimeSinceLastWarpUpdate -= warpTickRate;
                currentSpinRate = spinRate.GetNaiveWarpFactor((float)songPosition);
                currentTimeWarp = timeWarp.GetNaiveWarpFactor((float)songPosition);
            }
            else
            {
                var prevT = songPosition - songElapsed;
                currentSpinRate = spinRate.GetAccurateWarpFactor(prevT, songPosition);
                currentTimeWarp = timeWarp.GetAccurateWarpFactor(prevT, songPosition);
            }
        }

        protected virtual void UpdateSlomo()
        {
            if (NoisEvader.Desktop.IsMouseOverGUI)
                return;

            if (inputHelper.MouseLeftPressed())
                SetSlomo(true);
            else if (inputHelper.MouseLeftReleased())
                SetSlomo(false);
        }

        private void UpdateLoading(GameTime gameTime)
        {
            camera.Update(gameTime);
            var levelTime = new LevelTime()
            {
                GameTime = gameTime,
                SongPosition = 0,
                GameSpeed = 0,
                TimeWarp = 0,
            };
            UpdatePlayer(levelTime);

            if (inputHelper.KeyPressed(Keys.Escape))
            {
                Paused = true;
                return;
            }

            if (audioLoaded)
            {
                logger.Info("Starting level");

                // keeping this stuff disabled until I can make UpdateGameObjects
                // run fast enough for this to make sense.
                /*
                if (ActiveMod.Flags.HasFlag(ModFlags.Showcase) || ActiveMod.Flags.HasFlag(ModFlags.Auto))
                {
                    posScreen.Position.Minimum = 0;
                    posScreen.Position.Maximum = (float)Audio.TotalTime.TotalSeconds;
                    posScreen.Show();
                }
                */

                Audio.Play();
                State = ArenaState.Playing;
            }
        }

        private void UpdatePaused()
        {
            if (inputHelper.KeyPressed(Keys.Escape))
                Paused = false;
        }

        private void UpdateScoreScreen(GameTime gameTime)
        {
            Audio.Update(GameSpeed);
            if (inputHelper.KeyPressed(Keys.Escape) || inputHelper.KeyPressed(Keys.Enter))
            {
                ExitToMenu();
            }
            var levelTime = new LevelTime()
            {
                GameTime = gameTime,
                SongPosition = songPosition,
                GameSpeed = GameSpeed,
                TimeWarp = currentTimeWarp,
            };
            outerRings.Update(levelTime);
            particles.Update(levelTime);
            screenShake.Update(levelTime);
        }

        private void UpdateGameObjects(LevelTime levelTime)
        {
            outerRings.Update(levelTime);
            spawners.Update(levelTime);
            UpdatePlayer(levelTime);
            flashlight?.Update(player.Center);
            var hit = bulletMgr.Update(levelTime, player, particles, slomo);
            if (hit != CollisionType.None)
                CollisionOccurred();
            UpdateProgressCircles();

            particles.Update(levelTime);
            screenShake.Update(levelTime);

            replayRecorder?.AddPlayerPos(player.Center);
        }

        private void UpdateSkipIntro()
        {
            var possible = SkipIntroPossible();
            skipIntroText.Visible = possible;
            if (possible && inputHelper.KeyPressed(Keys.Space))
            {
                SkipTo(firstShotTime - acclimation);
            }

            bool SkipIntroPossible() =>
                firstShotTime > minIntroDuration
                && firstShotTime - acclimation > Audio.Position;
        }

        private Shot GetFirstShot() =>
            level.Script.Shots.Find(x => x.Amount0 != 0);

        private void SkipTo(float position)
        {
            var oldPosition = (float)Audio.Position;
            logger.Info("Skipping from {a} to {b}", oldPosition, position);
            const float step = 1000f / 30f;
            var elapsed = TimeSpan.FromMilliseconds(step);

            if (oldPosition > position)
            {
                oldPosition = 0;
            }

            for (float i = oldPosition; i <= position; i += step)
            {
                var total = TimeSpan.FromMilliseconds(i);
                var fakeGameTime = new GameTime(total, elapsed);

                var sr = spinRate.GetAccurateWarpFactor((total - elapsed).TotalMilliseconds, i);
                var tw = timeWarp.GetAccurateWarpFactor((total - elapsed).TotalMilliseconds, i);

                var fakeLevelTime = new LevelTime()
                {
                    GameTime = fakeGameTime,
                    SongPosition = i,
                    GameSpeed = GameSpeed,
                    TimeWarp = tw,
                    SpinRate = sr,
                };

                // these two are here for replays
                UpdateSlomo();
                camera.Update(fakeGameTime);

                UpdateGameObjects(fakeLevelTime);
            }
            Audio.JumpTo(position);
            songPosition = position;
            prevSongPosition = songPosition - step;
        }

        protected void SetSlomo(bool newSlomo)
        {
            if (slomo == newSlomo)
                return;

            var prevSlomo = slomo;
            slomo = newSlomo;
            Audio.PlaybackSpeed = GameSpeed;

            replayRecorder?.AddSlomo(slomo);

            if (slomo)
            {
                slomoStart = songPosition;
                camera.SlomoZoomIn();
            }
            else
            {
                totalSlomoTime += songPosition - slomoStart;
                camera.SlomoZoomOut();
            }
        }

        protected virtual void UpdatePlayer(LevelTime levelTime)
        {
            player.Update(levelTime, arena.Circle, camera, ActiveMod.Flags.HasFlag(ModFlags.Live));

            if (ActiveMod.Flags.HasFlag(ModFlags.Showcase))
                return;

            var colls = bulletMgr.CheckPlayerCollisions(player);
            if (colls.Contains(CollisionType.Bad))
                CollisionOccurred();
            else if (colls.Contains(CollisionType.Heart))
                replayRecorder?.AddHeart();
        }

        protected virtual void CollisionOccurred()
        {
            if (player.Invincibility != InvincibilityType.None)
                return;

            if (player.HasHeart)
            {
                player.HeartLost();
            }
            else
            {
                player.Hit();

                if (Config.GameSettings.DrawParticles)
                    particles.CreatePlayerHitParticles(player.Center);

                DoHitScreenShake();
            }

            replayRecorder.AddHit(player.Invincibility);
        }

        protected void DoHitScreenShake()
        {
            if (Config.GameSettings.ScreenShakeOnHit)
            {
                const int Magnitude = 6;
                const int Duration = 600;
                screenShake.Shake(Magnitude, Duration);
            }
        }

        /// <summary>
        /// Updates score and song position circles in the center of the arena.
        /// </summary>
        private void UpdateProgressCircles()
        {
            var songPosPercent = Math.Min(1, Audio.PositionPercent);

            arena.SongPosCircle.Radius = (float)(songPosPercent * arena.Circle.Radius);
            arena.SongPosCircle.Center = arena.Circle.Center;

            if (bulletMgr.TotalExited > 0)
                arena.ScoreCircle.Radius = bulletMgr.ScorePercent * arena.SongPosCircle.Radius;
            else
                arena.ScoreCircle.Radius = arena.SongPosCircle.Radius;

            arena.ScoreCircle.Center = arena.Circle.Center;
        }

        /// <summary>
        /// Switches to the score screen and saves the score.
        /// </summary>
        private void EndLevel()
        {
            logger.Info("Level ended");
            State = ArenaState.ScoreScreen;

            var finalScore = bulletMgr.ScorePercent;
            int displayScore;
            if (finalScore == 1 || bulletMgr.TotalExited == 0)
                displayScore = 100;
            else if (finalScore >= 0.99) // special case - don't show 100 for 99.6
                displayScore = 99;
            else
                displayScore = (int)Math.Round(finalScore * 100, MidpointRounding.AwayFromZero);

            scoreScreen.Score.TextColor = Color.White;
            scoreScreen.Score.Text = $"{displayScore}%";
            // generating a font instead of just using an existing one allows me
            // to render text at arbitrary sizes
            var size = (int)(arena.Circle.Radius * 1.1f * bulletMgr.ScorePercent);
            scoreScreen.Score.Font = Fonts.Orkney.GetFont(Fonts.PtToPx(size));
            scoreScreen.Show();

            outerRings.SpawnNewRings = false;

            if (ActiveMod.TickRate.HasValue)
                TickLimitChanged?.Invoke(this, null);

            if (player is HumanPlayer p)
            {
                replayRecorder.Replay.FinalScorePercent = finalScore;
                var replayFilename = replayRecorder.Replay.Timestamp.ToUnix().ToString()
                    + Replay.ReplayExtension;
                if (!ActiveMod.Flags.HasFlag(ModFlags.Showcase) && !ActiveMod.Flags.HasFlag(ModFlags.Zen))
                {
                    score = new Score()
                    {
                        XmlHash = level.XmlHash,
                        Time = replayRecorder.Replay.Timestamp,
                        Percent = finalScore,
                        Mod = ActiveMod,
                        HeartGotten = p.HasHeart,
                        TotalHits = p.TotalHits,
                        TotalSlomoTime = (float)totalSlomoTime,
                        ReplayFile = replayFilename,
                    };
                    Task.Run(() =>
                    {
                        Database.SaveScore(score);
                        replayRecorder.Replay.Save(Path.Combine(Replay.ReplaysDir, replayFilename));
                    });
                }
            }
        }

        public void ExitToMenu()
        {
            if (Audio.Position > PlayThreshold && !ActiveMod.Flags.HasFlag(ModFlags.Showcase)
                && !ActiveMod.Flags.HasFlag(ModFlags.Auto)
                && player is HumanPlayer)
            {
                Database.IncrementPlaycount(level.XmlHash);
            }

            Audio.Stop();

            if (ActiveMod.TickRate.HasValue)
                TickLimitChanged?.Invoke(this, null);

            ExitedToMenu?.Invoke(this, score);
        }

        public virtual void Draw(GraphicsDevice gd, SpriteBatch spriteBatch, DrawBatch drawBatch)
        {
            if (State == ArenaState.Idle)
                return;

            if (State == ArenaState.ScoreScreen)
            {
                DrawScoreScreen(drawBatch);
                return;
            }

            // 1) to rendertargets
            if (flashlight != null)
                RenderFlashlight(gd, drawBatch);

            // 2) to backbuffer
            if (ShouldDrawPlayer())
                player.DrawShadow(drawBatch);

            outerRings.Draw(drawBatch);

            arena.DrawGlow(drawBatch);

            spawners.DrawFlares(drawBatch);

            arena.Draw(drawBatch);

            spawners.Draw(drawBatch);

            if (skipIntroText.Visible)
                DrawWithoutTransform(spriteBatch, drawBatch,
                    () => skipIntroText.Draw(spriteBatch));

            if (ShouldDrawPlayer())
                player.Draw(drawBatch);

            if (State == ArenaState.Playing)
                bulletMgr.Draw(spriteBatch, drawBatch);

            particles.Draw(drawBatch);

            DrawOverlaysBase(spriteBatch, drawBatch);
        }

        private void RenderFlashlight(GraphicsDevice gd, DrawBatch drawBatch)
        {
            gd.SetRenderTarget(flashlightTarget);
            gd.Clear(FlashlightVignette.BackgroundColor);
            drawBatch.End();
            drawBatch.BeginWithMySettings(ScreenTransform, null, true);
            flashlight.Draw(drawBatch);
            drawBatch.End();
            gd.SetRenderTarget(null);
            drawBatch.BeginWithMySettings(ScreenTransform, null);
        }

        private void DrawScoreScreen(DrawBatch drawBatch)
        {
            outerRings.Draw(drawBatch);
            arena.Draw(drawBatch);
            particles.Draw(drawBatch);
        }

        private void DrawOverlaysBase(SpriteBatch spriteBatch, DrawBatch drawBatch)
        {
            drawBatch.End();
            spriteBatch.End();
            spriteBatch.BeginWithMySettings(null, null);
            drawBatch.BeginWithMySettings(null, null);

            DrawOverlays(spriteBatch);

            drawBatch.End();
            spriteBatch.End();
            spriteBatch.BeginWithMySettings(camera.Transform, null);
            drawBatch.BeginWithMySettings(null, null);
        }

        protected virtual void DrawOverlays(SpriteBatch spriteBatch)
        {
            if (flashlight != null)
                spriteBatch.Draw(flashlightTarget, Vector2.Zero, Color.White);

            // F3 text
            if (debugText.Visible)
            {
                var debugInfo = "";

                debugInfo += TimeSpan.FromMilliseconds(songPosition).ToString(@"mm\:ss\.FF") + "\n";

                debugInfo += "\n";
                debugInfo += $"GS: {GameSpeed:0.###}\n";
                debugInfo += $"SR: {currentSpinRate:0.###}\n";
                debugInfo += $"TW: {currentTimeWarp:0.###}\n";

                debugInfo += "\n";
                debugInfo += $"{bulletMgr.Bullets.Count} bullets on screen\n";
                debugInfo += $"{bulletMgr.UpcomingShots} upcoming shots\n";
                debugInfo += $"{bulletMgr.ActiveStreams.Count} active streams\n";

                debugInfo += "\n";
                debugInfo += $"{bulletMgr.TotalExited} exited total\n";
                debugInfo += $"{bulletMgr.Score} score ({bulletMgr.ScorePercent * 100:0.00}%)\n";

                debugInfo += "\n";
                debugInfo += $"{particles.Count} particles\n";
                debugInfo += $"{outerRings.RingCount} outer rings\n";

                debugInfo += "\n";
                if (player.Invincibility == InvincibilityType.Eternal)
                    debugInfo += "hax enabled";

                debugText.Text = debugInfo;
                debugText.Draw(spriteBatch);
            }
        }

        protected void DrawWithoutTransform(SpriteBatch spriteBatch, DrawBatch drawBatch, Action action)
        {
            drawBatch.End();
            spriteBatch.End();
            spriteBatch.BeginWithMySettings(null, null);
            action.Invoke();
            spriteBatch.End();
            spriteBatch.BeginWithMySettings(camera.Transform, null);
            drawBatch.BeginWithMySettings(camera.Transform, null);
        }

        /// <summary>
        /// Determines if the player should be drawn.
        /// </summary>
        private bool ShouldDrawPlayer() => !ActiveMod.Flags.HasFlag(ModFlags.Showcase)
                || Mouse.GetState().RightButton == ButtonState.Pressed;

        public void OnWindowResized()
        {
            camera.SetMainScale();
            camera.SetTransform(arena.Circle.Center);
        }
    }

    public enum ArenaState
    {
        Idle,
        Loading,
        Playing,
        ScoreScreen,
    }
}
