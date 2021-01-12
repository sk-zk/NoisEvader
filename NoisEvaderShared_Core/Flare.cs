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
    public class Flare
    {
        private const float BaseOffset = (float)(11.2 * MathEx.DegToRad);
        private const float TopOffset = (float)(11.2 * MathEx.DegToRad);

        public Color Color { get; set; }

        public Color CurrentColor
        {
            get
            {
                var current = Color;
                if (ani.Progress < 1)
                    current = Color.Lerp(Color, Color.Transparent, ani.CurrentValue);
                return current;
            }
        }

        private float angle;

        protected Vector2 arenaCenter;

        public bool HasFadedOut => ani.Progress >= 1;

        private Tweener ani;

        private bool hasBeenUpdatedOnce;

        /// <summary>
        /// When in the level this flare has been created; needed for drawing them oldest to newest.
        /// </summary>
        public float StartTime { get; }

        public Flare(Color color, float fadeoutDelay, Vector2 arenaCenter, float startTime)
        {
            Color = color;
            this.arenaCenter = arenaCenter;
            StartTime = startTime;

            ani = new Tweener((x) => x);
            ani.Animate(0, 1, 533, startDelay: fadeoutDelay);
        }

        public void Update(LevelTime levelTime, float angle)
        {
            if (HasFadedOut)
                return;
            hasBeenUpdatedOnce = true;

            ani.Update((float)levelTime.LevelElapsedMs);

            this.angle = angle;
        }

        public void Draw(DrawBatch drawBatch)
        {
            if (HasFadedOut || !hasBeenUpdatedOnce)
                return;

            var a = MathEx.PointOnCircumference(arenaCenter, Arena.ArenaRadius,
                angle - BaseOffset);
            var b = MathEx.PointOnCircumference(arenaCenter, Arena.ArenaRadius,
                angle + BaseOffset);
            var d = MathEx.PointOnCircumference(arenaCenter, NoisEvader.ScreenBounds.Width,
                angle - TopOffset);
            var c = MathEx.PointOnCircumference(arenaCenter, NoisEvader.ScreenBounds.Width,
                angle + TopOffset);

            drawBatch.FillQuad(new SolidColorBrush(CurrentColor), new[] { a, b, c, d }, 0);
        }

    }
}
