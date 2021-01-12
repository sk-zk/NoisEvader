using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoisEvader.UI
{
    static class UIUtil
    {
        public static void SetHeart(Image heartImg, int height)
        {
            var heartHeight = height;
            var heartWidth = heartHeight * ((float)HeartSprite.Texture.Width / HeartSprite.Texture.Height);
            heartImg.Width = (int)heartWidth;
            heartImg.Height = (int)heartHeight;
            heartImg.Margin = new Thickness(0, 0, 8, 0);
            heartImg.VerticalAlignment = VerticalAlignment.Center;
        }

        public static string GetDurationString(TimeSpan duration)
        {
            return duration.Hours > 0
                ? duration.ToString(@"h\:m\:ss")
                : duration.ToString(@"m\:ss");
        }

        public static string GetDifficultyString(int difficulty)
        {
            // there is a level which has a difficulty of 40000.
            // the game froze for 30 seconds, then the runtime crashed.
            const int diffDisplayCap = 500;
            difficulty = Math.Min(difficulty, diffDisplayCap);

            var diff = new string('●', difficulty);
            diff = diff.PadRight(5, '○');
            return diff;
        }

        public static string GetSubtitle(LevelInfo info) =>
            info.AdvancedFlag ? "-advanced-" : info.Subtitle?.Trim();

    }
}
