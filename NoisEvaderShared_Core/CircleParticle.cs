using LilyPath;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoisEvader
{
    public class CircleParticle : CircleSprite
    {
        public Vector2 Velocity { get; set; }

        private bool firstUpdate = true;

        private float startRadius;

        public override float Radius
        {
            get => base.Radius;
            set
            {
                base.Radius = value;
                if (firstUpdate)
                    startRadius = value;
            }
        }

        /// <summary>
        /// Radius decrease per second.
        /// </summary>
        public float ShrinkSpeed { get; set; }

        public void Update(LevelTime levelTime)
        {
            if (firstUpdate)
            {
                firstUpdate = false;
                return;
            }

            if (Radius == 0f)
                return;

            var elapsed = (float)levelTime.ElapsedMs;

            // shrink
            var center = Center;
            var shrink = ShrinkSpeed / 1000;
            Radius = Math.Max(0, Radius - (shrink * elapsed));
            Center = center;

            // move
            Center += Velocity * elapsed;
        }

        public override void Draw(DrawBatch drawBatch)
        {
            if (Radius < startRadius)
                base.Draw(drawBatch);
        }

    }
}
