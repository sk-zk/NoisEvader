using LilyPath;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    public class HomingGlow : Glow
    {
        private readonly int[] tris = new[] { 0, 1, 2, 0, 2, 3 };

        public HomingGlow(float speed) : base()
        {
            float glowAniSpeed = 3000 / speed;
            float glowAniEndDelay = 250 / speed;
            Animation.Animate(0f, 1.5f, glowAniSpeed, repeat: true, repeatDelay: glowAniEndDelay);
        }

        public void Draw(DrawBatch batch, Homing homing, Color color)
        {
            var pos1 = Math.Max(0f, Animation.CurrentValue - 0.5f);
            var pos2 = Math.Min(1f, Animation.CurrentValue);

            //       D
            //       /\
            //      /  \
            //   C /....\  A
            //     \    /
            //      \  /
            //       \/
            //       B

            var A = homing.Position + homing.GetCorner(0);
            var B = homing.Position + homing.GetCorner(1);
            var C = homing.Position + homing.GetCorner(2);
            var D = homing.Position + homing.GetCorner(3);

            var diag = C - A; // animation is two points moving along this line

            var point1 = A + diag * pos1;
            var point2 = A + diag * pos2;

            var colorBlendAmount = Animation.Progress < 0.5f
                ? Animation.Progress : 1 - Animation.Progress;
            var colorBlend = MathHelper.Lerp(1f, 0.1f, colorBlendAmount);
            var glowColor = Color.Lerp(BaseColor, color, colorBlend);

            batch.FillTriangleList(new SolidColorBrush(glowColor),
                new Vector2[] { point1, B, point2, D }, tris);
        }
    }
}
