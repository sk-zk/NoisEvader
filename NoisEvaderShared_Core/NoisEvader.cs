using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Myra.Graphics2D.UI;
using Myra;
using LilyPath;
using System.Diagnostics;
using Myra.Graphics2D.UI.Styles;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonoGame.Extended.Screens;
using NLog.Fluent;
using NLog;
using NoisEvader.UI;

namespace NoisEvader
{
    public partial class NoisEvader : Game
    {
        public static Rectangle ScreenBounds { get; private set; }

        public static Vector2 ScreenCenter =>
            new Vector2(ScreenBounds.Width / 2, ScreenBounds.Height / 2);

        // nuclear option. just let me construct rendertargets pls thx.
        // todo: come up with a better way to do this.
        public static GraphicsDevice Gd { get; private set; }

        public const float GameWidth = 896;
        public const float GameHeight = 504;

        private GraphicsDeviceManager graphics;
        private const int MultiSampleCount = 16;

        private SpriteBatch spriteBatch;
        private DrawBatch drawBatch;
        private GameState gameState;
        private Arena Arena;

        private AudioPlayer audioPlayer = AudioPlayer.Instance;
        private InputHelper inputHelper = new InputHelper();

        private MenuScene menuScene;

        private SimpleText fpsText;
        private readonly FpsCounter fpsCounter = new FpsCounter();

        private bool vsyncInMenus = true;

        // to force the update method to run 30/sec
        // for compatibility with certain sdgr+ levels 
        // that break with other framerates.
        private double? tickRate; // in ms

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public NoisEvader()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Config.LoadSettings();

            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferredBackBufferWidth = Config.GameSettings.ScreenWidth;
            graphics.PreferredBackBufferHeight = Config.GameSettings.ScreenHeight;
            graphics.IsFullScreen = Config.GameSettings.WindowType != WindowType.Windowed;
            graphics.HardwareModeSwitch = Config.GameSettings.WindowType == WindowType.Fullscreen;
            #if WINDOWS
                // Setting VSync in fullscreen at runtime crashes in DX,
                // so we can't have vsync enabled in menus to 
                // prevent having to do that
                vsyncInMenus = Config.GameSettings.WindowType != WindowType.Fullscreen;
                graphics.SynchronizeWithVerticalRetrace = Config.GameSettings.Vsync;
            #endif
            graphics.PreferMultiSampling = true;
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;

            logger.Info("Setting graphics properties");
            graphics.ApplyChanges();

            IsMouseVisible = false;
            IsFixedTimeStep = false;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            logger.Info("Initializing game");

            GraphicsDevice.PresentationParameters.MultiSampleCount = MultiSampleCount;
            graphics.ApplyChanges();

            ScreenBounds = graphics.GraphicsDevice.Viewport.Bounds;
            Gd = GraphicsDevice;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            spriteBatch = new SpriteBatch(GraphicsDevice);
            drawBatch = new DrawBatch(GraphicsDevice);

            gameState = GameState.LoadingScreen;

            Database.EnsureExists();

            logger.Info("Generating textures");
            TextureGenerator.LoadStaticAssets(GraphicsDevice, Content);
            Fonts.Content.Init(Content);
            Fonts.Init();

            fpsText = new SimpleText()
            {
                Font = Fonts.Content.DebugFont,
                Color = Color.Gray,
                XOrigin = XOrigin.Right
            };

            menuScene = new MenuScene();

            InitGui();
            InitScreenshotTarget();
        }

        private void InitScreenshotTarget()
        {
            screenshotTarget = new RenderTarget2D(GraphicsDevice, 
                ScreenBounds.Width, ScreenBounds.Height, 
                false, SurfaceFormat.Color, DepthFormat.None, 
                GraphicsDevice.PresentationParameters.MultiSampleCount,
                RenderTargetUsage.DiscardContents);
        }

        private void InitArena(Arena arena)
        {
            arena.ArenaPaused += Arena_PausePressed;
            arena.ArenaUnpaused += Arena_ArenaUnpaused;
            arena.ExitedToMenu += Arena_ExitedToMenu;
            arena.TickLimitChanged += Arena_TickLimitChanged;
        }

        private void Arena_TickLimitChanged(object sender, float? ticksPerSecond)
        {
            if (ticksPerSecond.HasValue)
            {
                logger.Info("Set time step to {ticks} ticks", ticksPerSecond);
                IsFixedTimeStep = true;
                TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0 / ticksPerSecond.Value);
            }
            else
            {
                logger.Info("Fixed time step disabled");
                IsFixedTimeStep = false;
            }
            tickRate = 1000.0 / ticksPerSecond;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            logger.Info("Unloading content");
            audioPlayer.Dispose();
            NLog.LogManager.Shutdown();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            UpdateScreenBoundsProperty();
            UpdateUI();
            inputHelper.Update();
            screenshotThisFrame = inputHelper.KeyPressed(Keys.F12);

            switch (gameState)
            {
                case GameState.LoadingScreen:
                    SwitchToMainMenu();
                    break;
                case GameState.Menu:
                    UpdateMenu(gameTime);
                    break;
                case GameState.PlayingLevel:
                    UpdatePlaying(gameTime);
                    break;
            }

            base.Update(gameTime);
        }

        private void UpdateMenu(GameTime gameTime)
        {
            if (Desktop.Root == mainMenu)
                menuScene.Update(gameTime);

            CheckSettingsKeyCombo();
        }

        private void CheckSettingsKeyCombo()
        {
            if (inputHelper.KeyComboPressed(Keys.LeftControl, Keys.O))
                ShowSettings();
        }

        private void UpdateScreenBoundsProperty()
        {
            ScreenBounds = graphics.GraphicsDevice.Viewport.Bounds;
        }

        private void UpdateUI()
        {
            if (Desktop.Root != null)
                Desktop.Root.Enabled = IsActive;
            Window.AllowUserResizing = Config.GameSettings.WindowType == WindowType.Windowed;
            levelSelectScreen.Update();
        }

        private void UpdatePlaying(GameTime gameTime)
        {
            if (inputHelper.KeyPressed(Keys.R))
            {
                RestartCurrentLevel();
                return;
            }

            try
            {
                Arena.Update(gameTime);
                IsMouseVisible = (Arena.State == ArenaState.ScoreScreen || Arena.Paused);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Unhandled exception in update loop");
                Arena.ExitToMenu();
                ShowErrorMessage("Something just broke.\nCheck the game log for details.");
            }
        }

        private void SetVsync(bool enabled)
        {
            #if WINDOWS
                // This would crash in DX if fullscreened
                if(graphics.HardwareModeSwitch)
                    return;
            #endif

            if (graphics.SynchronizeWithVerticalRetrace == enabled)
                return;

            logger.Debug((enabled ? "Enabling" : "Disabling") + " Vsync");
            graphics.SynchronizeWithVerticalRetrace = enabled;
            graphics.ApplyChanges();
            fpsCounter.Flush();
        }

        private bool screenshotThisFrame = false;
        private RenderTarget2D screenshotTarget;

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (screenshotThisFrame)
            {
                GraphicsDevice.SetRenderTarget(screenshotTarget);
            }

            Matrix transform = Matrix.Identity;
            switch (gameState)
            {
                case GameState.PlayingLevel:
                    GraphicsDevice.Clear(Arena.BackgroundColor);
                    transform = Arena.ScreenTransform;
                    break;
                case GameState.Menu:
                    if (Desktop.Root == mainMenu)
                        GraphicsDevice.Clear(MenuScene.BackgroundColor);
                    else
                        GraphicsDevice.Clear(Color.White);
                    transform = menuScene.Camera.Transform;
                    break;
                case GameState.LoadingScreen:
                    GraphicsDevice.Clear(MenuScene.BackgroundColor);
                    break;
            }

            spriteBatch.BeginWithMySettings(transform, null);
            drawBatch.BeginWithMySettings(transform, null);
            
            switch (gameState)
            {
                case GameState.LoadingScreen:
                    break;
                case GameState.Menu:
                    if (Desktop.Root == mainMenu)
                        menuScene.Draw(spriteBatch, drawBatch);
                    DrawUI(transform);
                    break;
                case GameState.PlayingLevel:
                    Arena.Draw(GraphicsDevice, spriteBatch, drawBatch);
                    DrawUI(transform);
                    break;
            }

            drawBatch.End();
            spriteBatch.End();

            spriteBatch.Begin();
            fpsCounter.Update(gameTime);
            DrawFps();
            spriteBatch.End();

            if (screenshotThisFrame)
            {
                SaveScreenshot(screenshotTarget);
                screenshotThisFrame = false;
                GraphicsDevice.SetRenderTarget(null);
                spriteBatch.Begin();
                spriteBatch.Draw(screenshotTarget, Vector2.Zero, Color.White);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        private void DrawUI(Matrix transform)
        {
            drawBatch.End();
            spriteBatch.End();

            try
            {
                Desktop.Render();
            }
            catch (NullReferenceException nrex)
            {
                // dumb myra bug. ignore and go again.
                if (nrex.TargetSite.Name == "get_ContainsTouch")
                {
                    Debug.WriteLine("LIFE IS PAIN");
                    Desktop.Render();
                }
                else if (nrex.TargetSite.Name == "OnTouchDown")
                {
                    Debug.WriteLine("WHY DOES THIS EVEN HAPPEN");
                    Desktop.Render();
                }
                else
                {
                    throw;
                }
            }
            catch (InvalidOperationException iex)
            {
                if (iex.Message.StartsWith("Collection was modified"))
                {
                    Desktop.Render();
                }
                else
                {
                    throw;
                }
            }

            spriteBatch.BeginWithMySettings(transform, null);
            drawBatch.BeginWithMySettings(transform, null);
        }

        private void SaveScreenshot(RenderTarget2D rt)
        {
            const string screenshotDir = "Screenshots";

            Task.Run(() =>
            {
                Directory.CreateDirectory(screenshotDir);

                string path;
                string filename;
                int i = 1;
                do
                {
                    filename = $"{DateTime.Now.ToUnix()}-{i}.png";
                    path = Path.Combine(screenshotDir, filename);
                    i++;
                }
                while (File.Exists(path));

                using var fs = new FileStream(path, FileMode.Create);
                rt.SaveAsPng(fs, rt.Width, rt.Height);

                logger.Log(LogLevel.Debug, "Writing screenshot " + filename);
            });
        }

        private void DrawFps()
        {
            if (!Config.GameSettings.ShowFps)
                return;

            fpsText.Text = $"{fpsCounter.AverageFps:F0}";
            fpsText.Position = new Vector2(ScreenBounds.Width - 10, 5);
            fpsText.Draw(spriteBatch);
        }

        public enum GameState
        {
            LoadingScreen,
            Menu,
            PlayingLevel
        }
    }
}

