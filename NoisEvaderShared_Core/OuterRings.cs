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
    class OuterRings
    {
        /// <summary>
        /// Toggles spawning new rings.
        /// </summary>
        public bool SpawnNewRings { get; set; } = true;

        public bool FadeIn { get; set; } = false;

        public int RingCount => rings.Count;

        private List<OuterRing> rings = new List<OuterRing>();
        private Color ringColor;

        public float SpawnRate { get; set; } = 500f; // in ms
        private double timeSinceLastSpawn;
        private float MaxRingRadius => NoisEvader.GameHeight * 4;

        public float Speed { get; set; } = 1;

        private CircleSprite arenaCircle;

        public OuterRings(Color ringColor, CircleSprite arenaCircle)
        {
            this.ringColor = ringColor;
            this.arenaCircle = arenaCircle;
        }

        private void SpawnNewRing()
        {
            rings.Add(new OuterRing
            {
                Radius = arenaCircle.Radius,
                Center = arenaCircle.Center,
                BorderColor = ringColor,
            });
        }

        public void Update(LevelTime lt)
        {
            // TODO: Fix move speed & spawn speed

            if (!Config.GameSettings.DrawOuterRings)
                return;

            var elapsed = lt.ElapsedMs;

            for (int i = 0; i < rings.Count; i++)
            {
                // despawn rings if they've been offscreen for too long
                if (rings[i].Radius > MaxRingRadius)
                {
                    rings.RemoveAt(i);
                    i--;
                    continue;
                }

                // update radius / line thickness of rings on screen
                var radiusSpeed = (float)(elapsed * 0.05 * Speed);
                rings[i].Radius += (float)(Math.Pow(rings[i].LineThickness / arenaCircle.LineThickness, 1.5)
                    * lt.TimeWarp * radiusSpeed);
                rings[i].LineThickness = arenaCircle.LineThickness * (rings[i].Radius / arenaCircle.Radius);
                rings[i].Center = arenaCircle.Center;
                var color = ringColor;
                if (FadeIn)
                {
                    var fadeInDuration = (float)(300 / lt.TimeWarp);
                    var alpha = 1f;
                    if(rings[i].Age < fadeInDuration)
                    {
                        var progress = rings[i].Age / fadeInDuration;
                        alpha = MathEx.EaseInOutSine(progress);
                    }
                    rings[i].BorderColor = new Color(color, alpha);
                }
                rings[i].Update(lt);
            }

            // determine if new rings should be spawned
            if (SpawnNewRings)
            {
                timeSinceLastSpawn += elapsed * Math.Min(1, lt.TimeWarp);
                if (lt.TimeWarp > 0.00001f && timeSinceLastSpawn > SpawnRate)
                {
                    SpawnNewRing();
                    timeSinceLastSpawn = 0;
                }
            }
        }

        public void Clear()
        {
            rings.Clear();
        }

        public void Draw(DrawBatch drawBatch)
        {
            if (!Config.GameSettings.DrawOuterRings)
                return;

            foreach (OuterRing ring in rings)
                ring.DrawBorderOnly(drawBatch);
        }

    }
}
