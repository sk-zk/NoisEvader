using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.TextureAtlases;
using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace NoisEvader
{
    /// <summary>
    /// Generates textures from SVG.
    /// </summary>
    public static class TextureGenerator
    {
        private static readonly Assembly assembly = Assembly.GetExecutingAssembly();

        /// <summary>
        /// Loads textures generated from SVG.
        /// MUST BE CALLED ON INIT.
        /// </summary>
        public static void LoadStaticAssets(GraphicsDevice graphicsDevice, ContentManager content)
        {
            // ingame
            var heartSvg = LoadSvg("NoisEvader.Content.Svg.HeartBullet.svg");
            SetHeartSprite(graphicsDevice, heartSvg);

            // UI
            UI.LevelSelect.HeartIcon = new TextureRegion(SvgToTexture2D(
                heartSvg, 64, graphicsDevice));

            UI.LevelSelect.HeartOutlineIcon = new TextureRegion(SvgToTexture2D(
                LoadSvg("NoisEvader.Content.Svg.HeartOutline.svg"), 64, graphicsDevice));

            UI.MainMenu.LogoTexture = new TextureRegion(SvgToRelativeTexture2D(
                LoadSvg("NoisEvader.Content.Svg.TempLogo.svg"), 0.1, graphicsDevice));
        }

        private static SvgDocument LoadSvg(string resourceName)
        {
            var xml = new XmlDocument();
            xml.LoadXml(LoadResource(resourceName));
            var svg = SvgDocument.Open(xml);
            return svg;
        }

        private static string LoadResource(string resourceName)
        {
            var stream = assembly.GetManifestResourceStream(resourceName);
            using var sr = new StreamReader(stream);
            return sr.ReadToEnd();
        }

        private static void SetHeartSprite(GraphicsDevice graphicsDevice, SvgDocument heartSvg)
        {
            var heartTxRelSize = (Heart.HeartSize / NoisEvader.GameHeight) * 4;
            HeartSprite.Texture = SvgToRelativeTexture2D(heartSvg,
                heartTxRelSize, graphicsDevice);

            var verts = LoadResource("NoisEvader.Content.HeartBullet.shape")
                .Split("\n")
                .Select(x => {
                    var coords = x.Split(" ");
                    return new Vector2(
                        float.Parse(coords[0], CultureInfo.InvariantCulture),
                        float.Parse(coords[1], CultureInfo.InvariantCulture)
                        );
                    })
                .ToList();
            var triangulator = new LilyPath.Triangulator();
            triangulator.Triangulate(verts, 0, verts.Count);
            var tris = new int[triangulator.ComputedIndexCount];
            Array.Copy(triangulator.ComputedIndexes, tris, triangulator.ComputedIndexCount);
            HeartSprite.SetShape(verts, tris);
        }

        /// <summary>
        /// Converts an SVG document into a Texture2D with the correct size for the current screen resolution.
        /// </summary>
        public static Texture2D SvgToRelativeTexture2D(SvgDocument svgImage, double relativeSize,
            GraphicsDevice graphicsDevice)
        {
            var svgAspectRatio = (double)svgImage.Width / svgImage.Height;
            var height = relativeSize * graphicsDevice.Viewport.Height;
            var width = svgAspectRatio * height;

            return SvgToTexture2D(svgImage, width, height, graphicsDevice);
        }

        /// <summary>
        /// Converts an SVG document into a Texture2D.
        /// </summary>
        public static Texture2D SvgToTexture2D(SvgDocument svgImage, double width,
            GraphicsDevice graphicsDevice)
        {
            var height = width / ((double)svgImage.Width / svgImage.Height);
            return SvgToTexture2D(svgImage, width, height, graphicsDevice);
        }

        /// <summary>
        /// Converts an SVG document into a Texture2D.
        /// </summary>
        public static Texture2D SvgToTexture2D(SvgDocument svgImage, double width, double height,
            GraphicsDevice graphicsDevice, bool scaleSvg = true)
        {
            if (width < 1 || height < 1)
                return new Texture2D(graphicsDevice, 1, 1);

            Bitmap image = new Bitmap((int)width, (int)height, PixelFormat.Format32bppArgb);

            svgImage.ShapeRendering = SvgShapeRendering.GeometricPrecision;
            using (ISvgRenderer r = SvgRenderer.FromImage(image))
            {
                if (scaleSvg)
                {
                    float scale = image.Height / svgImage.Height; // to make the svg fit the dimensions of the img.
                    r.ScaleTransform(scale, scale);
                }
                r.SmoothingMode = SmoothingMode.AntiAlias;
                svgImage.Draw(r);
            }

            int bufferSize = image.Height * image.Width * 4; // rgba = 4 bytes
            MemoryStream memoryStream = new MemoryStream(bufferSize);
            image.Save(memoryStream, ImageFormat.Png);
            //image.Save(@"d:\temp\" + Path.GetRandomFileName() + ".png", ImageFormat.Png);
            return FromStreamPremultiplied(graphicsDevice, memoryStream);
        }

        /// <summary>
        /// Loads texture data from a stream and premultiplies alpha.
        /// </summary>
        private static Texture2D FromStreamPremultiplied(GraphicsDevice graphicsDevice, Stream stream)
        {
            // via https://gist.github.com/Layoric/6255384
            Texture2D texture = Texture2D.FromStream(graphicsDevice, stream);
            var data = new Microsoft.Xna.Framework.Color[texture.Width * texture.Height];
            texture.GetData(data);
            for (int i = 0; i != data.Length; ++i)
                data[i] = Microsoft.Xna.Framework.Color.FromNonPremultiplied(data[i].ToVector4());
            texture.SetData(data);
            return texture;
        }

    }
}
