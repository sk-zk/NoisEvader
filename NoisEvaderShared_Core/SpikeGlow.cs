using System;
using System.Collections.Generic;
using System.Text;
using LilyPath;
using Microsoft.Xna.Framework;

namespace NoisEvader
{
    public class SpikeGlow : Glow
    {
        public SpikeGlow(float speed) : base()
        {
            float glowAniSpeed = 2000 / speed;
            float glowAniEndDelay = 500 / speed;
            Animation.Animate(0f, 1, glowAniSpeed, repeat: true, repeatDelay: glowAniEndDelay);
        }

        public void Draw(DrawBatch batch, Vector2[] rotShape, Color color)
        {
            const float epsilon = 0.001f;
            var progress = Animation.Progress;
            if (progress == 1)
                return;

            // how much of the curve to draw:
            float curvePointPos =
                (rotShape.Length - 1
                - 1) // ignore tip
                * (1 - progress);

            // get needed points from shape
            int neededPointsAmt = (int)Math.Ceiling(curvePointPos);
            var glowShape = new Vector2[neededPointsAmt + 2];
            var startCopying = rotShape.Length - neededPointsAmt - 1;
            Array.Copy(rotShape, startCopying, glowShape, 1, neededPointsAmt + 1);
            glowShape[0] = rotShape[0]; // copy tip

            // interp last point to smooth the animation
            var decimals = curvePointPos - (int)curvePointPos;
            if (decimals > epsilon)
            {
                var A = glowShape[1];
                var B = glowShape[2];
                var dist = B - A;
                var end = B - (dist * decimals);
                glowShape[1] = end;
            }

            // triangulate with triangle fan.
            var faceAmt = neededPointsAmt;
            var triIdx = 0;
            var tris = new int[faceAmt * 3];
            for (int i = 1; i < faceAmt; i++)
            {
                tris[triIdx++] = 0;
                tris[triIdx++] = i;
                tris[triIdx++] = i + 1;
            }

            color = Color.Lerp(color, BaseColor, Animation.CurrentValue);
            batch.FillTriangleList(new SolidColorBrush(color), glowShape, tris);
        }
    }
}
