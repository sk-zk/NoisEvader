using LilyPath;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    public class BubbleGlow : Glow
    {
        private CircleSprite bubbleCircle;
        private float rotation;

        public BubbleGlow(float speed, float rotation, CircleSprite bubbleCircle) : base()
        {
            this.bubbleCircle = bubbleCircle;
            this.rotation = rotation;

            float glowAniSpeed = 4000 / speed;
            float glowAniEndDelay = 250 / speed;
            Animation.Animate(0f, 2, glowAniSpeed, repeat: true, repeatDelay: glowAniEndDelay);
        }

        public void Draw(DrawBatch batch, Color color)
        {
            var current = Animation.CurrentValue;
            var colorBlend = MathHelper.Lerp(1f, 0.4f, Animation.Progress);
            var glowColor = Color.Lerp(BaseColor, color, colorBlend);

            if (current < 1)
            {
                batch.FillCrescent(new SolidColorBrush(glowColor),
                    bubbleCircle.Center, bubbleCircle.Radius,
                    rotation, current, bubbleCircle.Subdivisions);
            }
            else
            {
                batch.FillCrescent(new SolidColorBrush(glowColor),
                    bubbleCircle.Center, bubbleCircle.Radius,
                    rotation - MathHelper.ToRadians(180), 1 - (current - 1), bubbleCircle.Subdivisions);
            }
        }
    }
}
