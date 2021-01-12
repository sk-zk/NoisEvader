using LilyPath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoisEvader
{
    public static class Extensions
    {
        public static void BeginWithMySettings(this SpriteBatch spriteBatch, Matrix? transformMatrix, Effect effect)
        {
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.AnisotropicClamp,
                null,
                new RasterizerState { MultiSampleAntiAlias = true },
                effect,
                transformMatrix
                );
        }

        public static void BeginWithMySettings(this DrawBatch drawBatch, Matrix? transformMatrix, Effect effect,
            bool opaqueBlend = false)
        {
            drawBatch.Begin(
                DrawSortMode.Deferred,
                opaqueBlend ? BlendState.Opaque : BlendState.AlphaBlend,
                SamplerState.AnisotropicClamp,
                null,
                new RasterizerState { MultiSampleAntiAlias = false, CullMode = CullMode.None },
                effect,
                transformMatrix ?? Matrix.Identity
                );
        }

        public static double Next(this Random random, double min, double max)
        {
            if (min == max)
                return min;

            return (random.NextDouble() * (max - min)) + min;
        }

        public static Color Invert(this Color color) =>
            new Color(color.PackedValue ^ 0xffffff);

        public static bool ContainsAllChars(this SpriteFont font, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

            var chars = font.Characters;
            for (int i = 0; i < text.Length; i++)
            {
                if (!chars.Contains(text[i]))
                    return false;
            }
            return true;
        }

        public static long ToUnix(this DateTime dateTime) =>
            new DateTimeOffset(dateTime).ToUnixTimeSeconds();

    }
}
