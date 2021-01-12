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
    public class OuterRing : CircleSprite
    {
        public float Age { get; private set; } = 0;

        private bool visible;

        public OuterRing()
        {
            Subdivisions = 100;
        }

        public void Update(LevelTime lt)
        {
            visible = RingOnScreen();
            Age += (float)lt.LevelElapsedMs;
        }

        public override void DrawBorderOnly(DrawBatch drawBatch)
        {
            if (!visible)
                return;

            base.DrawBorderOnly(drawBatch);
        }

        private bool RingOnScreen()
        {
            // the ring is visible up until its radius is larger than the 
            // distance from the center of the screen to one of its corners
            var M = NoisEvader.ScreenBounds.Size.ToVector2();
            var A = new Vector2(0, 0);
            var maxRadius = Vector2.Distance(M, A);
            return Radius < maxRadius;
        }

    }
}