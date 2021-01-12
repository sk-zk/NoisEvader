using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpriteFontPlus;
using FontStashSharp;

namespace NoisEvader
{
    public static class Fonts
    {
        /// <summary>
        /// Converts point to pixels.
        /// </summary>
        public static int PtToPx(float pt, float dpi = 96) =>
            (int)(pt * dpi / 72);

        public static FontSystem Orkney { get; private set; }
        public static FontSystem OrkneyWithFallback { get; private set; }
        public static FontSystem DejaVuSans { get; private set; }

        private const string OrkneyPath = "Content/Fonts/Orkney Regular.otf";
        private const string DejaVuSansPath = "Content/Fonts/DejaVuSans.ttf";
        private const string NotoSansJpPath = "Content/Fonts/NotoSansCJKjp-Regular.otf";

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Init()
        {
            logger.Info("Loading ttf fonts");

            DejaVuSans = CreateFontSystem(
                NoisEvader.Gd, 1024, 1024,
                DejaVuSansPath);

            Orkney = CreateFontSystem(
                NoisEvader.Gd, 1024, 1024,
                OrkneyPath);

            OrkneyWithFallback = CreateFontSystem(
                NoisEvader.Gd, 1024, 1024,
                OrkneyPath,
                DejaVuSansPath,
                NotoSansJpPath);
        }

        private static FontSystem CreateFontSystem(GraphicsDevice gd, int textureWidth, int textureHeight,
            params string[] fontPaths)
        {
            var system = FontSystemFactory.Create(gd, textureWidth, textureHeight);
            foreach (var font in fontPaths)
            {
                using var fs = new FileStream(font, FileMode.Open);
                system.AddFont(fs);
            }
            return system;
        }

        /// <summary>
        /// Fonts loaded through the MonoGame content pipeline.
        /// </summary>
        public static class Content
        {
            private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

            public static SpriteFont DebugFont { get; private set; }

            public static SpriteFont Orkney13 { get; private set; }

            public static void Init(ContentManager content)
            {
                logger.Info("Loading content pipeline fonts");

                DebugFont = content.Load<SpriteFont>("Fonts/DebugFont");
                Orkney13 = content.Load<SpriteFont>("Fonts/Orkney13");
            }
        }
    }

}
