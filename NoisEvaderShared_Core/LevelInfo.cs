using Microsoft.Xna.Framework;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace NoisEvader
{
    public class LevelInfo
    {
        public string Nick { get; set; }
        public string AudioFile { get; set; }
        public double Enemies { get; set; } // yes, that's a double. see ArenaSpawners.cs
        public LevelColors Colors { get; set; } = new LevelColors();
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Subtitle { get; set; }
        public int Difficulty { get; set; }
        public string Designer { get; set; }
        public bool HasHeart { get; set; }
        public int AudioPreviewPoint { get; set; }
        public bool AdvancedFlag { get; set; } = false; // adv field
        public bool ArenaGlowEnabled { get; set; }

        public LevelInfo() { }

        /// <summary>
        /// Parses the Info node of a Soundodger+ XML.
        /// </summary>
        public static LevelInfo ParseInfo(IDictionary<string, string> attribs, string xmlDir)
        {
            var i = new LevelInfo();

            i.Nick = attribs["nick"];

            i.Colors.Background = Color.White;

            if (attribs.TryGetValue("adv", out var adv))
            {
                var isAdv = bool.Parse(adv);
                i.Colors.Background = Color.Black;
                i.ArenaGlowEnabled = true;
                i.AdvancedFlag = isAdv;
            }
            else if (attribs.TryGetValue("bgBlack", out var bgBlack))
            {
                var hasBlackBg = bool.Parse(bgBlack);
                i.Colors.Background = hasBlackBg ? Color.Black : Color.White;
                i.ArenaGlowEnabled = hasBlackBg;
            }

            double enemies;
            if (!double.TryParse(attribs["enemies"], NumberStyles.Float, 
                CultureInfo.InvariantCulture, out enemies))
            {
                enemies = 0;
            }
            i.Enemies = enemies;

            i.Colors.Normal = ParseColor(attribs["color1"], i.AdvancedFlag);
            i.Colors.Normal2 = ParseColor(attribs["color2"], i.AdvancedFlag);
            i.Colors.Homing = ParseColor(attribs["color3"], i.AdvancedFlag);
            i.Colors.Bubble = ParseColor(attribs["color4"], i.AdvancedFlag);
            i.Colors.Outline = ParseColor(attribs["color5"], i.AdvancedFlag);
            i.Colors.OuterRings = ParseColor(attribs["color6"], i.AdvancedFlag);
            i.Colors.SlomoBackground = ParseColor(attribs["color7"], i.AdvancedFlag);
            i.Colors.ScoreCircle = ParseColor(attribs["color8"], i.AdvancedFlag);

            if (attribs.TryGetValue("color9", out var hugColor))
                i.Colors.Hug = ParseColor(hugColor, i.AdvancedFlag);
            else // level was made before dlc 3 so just set anything as hug color
                i.Colors.Hug = i.Colors.Normal;

            i.Title = attribs["title"];
            i.Artist = attribs["artist"];
            i.Difficulty = ParseInt(attribs["difficulty"], 0);
            i.Designer = attribs["designer"];

            if (attribs.TryGetValue("MP3Name", out var mp3name))
                i.AudioFile = mp3name;
            else
                i.AudioFile = "";

            if (attribs.TryGetValue("containsHeart", out var hasHeart))
                i.HasHeart = bool.Parse(hasHeart);

            if (attribs.TryGetValue("audioPreview", out var prevPoint))
                i.AudioPreviewPoint = ParseInt(prevPoint, 40);
            else
                i.AudioPreviewPoint = 40;

            if (attribs.TryGetValue("subtitle", out var subtitle))
                i.Subtitle = subtitle;

            return i;
        }

        private static int ParseInt(string input, int errorValue) =>
            int.TryParse(input, out int result) ? result : errorValue;

        /// <summary>
        /// Converts a string containing an int to a Color object.
        /// </summary>
        private static Color ParseColor(string colorString, bool invert)
        {
            var packedColor = ParseInt(colorString, 0x777777);
            var r = (packedColor >> 16) & 255;
            var g = (packedColor >> 8) & 255;
            var b = packedColor & 255;
            var color = new Color(r, g, b);
            if (invert)
                color = color.Invert();
            return color;
        }
    }

    public class LevelColors
    {
        public Color Normal { get; set; }
        public Color Normal2 { get; set; }
        public Color Homing { get; set; }
        public Color Bubble { get; set; }
        public Color Outline { get; set; }
        public Color OuterRings { get; set; }
        public Color SlomoBackground { get; set; }
        public Color ScoreCircle { get; set; }
        public Color Hug { get; set; }
        public Color Background { get; set; }

        public void Invert()
        {
            Normal = Normal.Invert();
            Normal2 = Normal2.Invert();
            Homing = Homing.Invert();
            Bubble = Bubble.Invert();
            Outline = Outline.Invert();
            OuterRings = OuterRings.Invert();
            SlomoBackground = SlomoBackground.Invert();
            ScoreCircle = ScoreCircle.Invert();
            Hug = Hug.Invert();
            Background = Background.Invert();
        }
    }
}
