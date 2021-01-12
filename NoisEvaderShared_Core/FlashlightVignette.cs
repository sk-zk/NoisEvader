using LilyPath;
using LilyPath.Pens;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    public class FlashlightVignette : IDrawable
    {
        private const int VisibleRadius = 75;
        public static readonly Color BackgroundColor = new Color(0x09, 0x09, 0x09, 0xff);
        private static readonly Pen GradientPen = new GradientPen(
            BackgroundColor, Color.Transparent, 8);

        private readonly CircleSprite circle;

        public FlashlightVignette()
        {
            circle = new CircleSprite
            {
                Radius = VisibleRadius,
                LineThickness = 0,
                FillColor = Color.Transparent,
            };
        }

        public void Update(Vector2 playerCenter)
        {
            circle.Center = playerCenter;
        }

        public void Draw(DrawBatch drawBatch)
        {
            circle.Draw(drawBatch);
            drawBatch.DrawCircle(GradientPen, circle.Center, circle.Radius, circle.Subdivisions);
        }
    }
}
