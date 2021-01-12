using LilyPath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoisEvader
{
    public class Spawner : CircleSprite
    {
        public const float SpawnerRadius = 25;

        // twitter.com/OneMrBean/status/661282757033652224
        /// <summary>
        /// How many degrees the spawner moves per second at speed 1.
        /// </summary>
        public const double RotationPerMs = 30.0 * MathEx.DegToRad / 1000.0;

        private float angle;
        public float Angle
        {
            get => angle;
            set
            {
                angle = MathHelper.WrapAngle(value);
                CalcPosition(Arena);
            }
        }

        /// <summary>
        /// The moment where the spawner disappears in ms.
        /// </summary>
        public float DespawnPoint { get; set; }

        public ArenaCircle Arena { get; set; }

        public List<Flare> ActiveFlares { get; set; } = new List<Flare>();

        private SpawnIndicator indicator;

        public Spawner() : base(Vector2.Zero, SpawnerRadius)
        {
            LineThickness = 2;
            FillColor = Color.White;

            Subdivisions = 40;

            indicator = new SpawnIndicator(this);
        }

        public Spawner(float angle, ArenaCircle arena)
            : this()
        {
            this.angle = angle;
            Arena = arena;
            CalcPosition(Arena);
        }

        protected virtual void CalcPosition(ArenaCircle arena)
        {
            var touchRadius = arena.Circle.Radius + Radius;
            var spwnX = arena.Circle.Center.X + (touchRadius * (float)Math.Cos(Angle)) - Radius;
            var spwnY = arena.Circle.Center.Y + (touchRadius * (float)Math.Sin(Angle)) - Radius;
            Position = new Vector2(spwnX, spwnY);
        }

        public bool IsStillInUse(double songPosition)
        {
            if (DespawnPoint == 0)
                return false;
            return DespawnPoint > songPosition;
        }

        public void AddToIndicatorList(double shotLaunchTime, Color color)
        {
            indicator.AddToIndicatorList(shotLaunchTime, color);
        }

        public virtual void Update(LevelTime levelTime, float spawnerSpeed)
        {
            UpdateAngle(spawnerSpeed);

            if (Math.Abs(angle) > 50.2654f) // 8*360
                angle = MathHelper.WrapAngle(angle);

            CalcPosition(Arena);

            indicator.Update(levelTime);

            UpdateFlares(levelTime);
        }

        protected virtual void UpdateFlares(LevelTime levelTime)
        {
            for (int i = 0; i < ActiveFlares.Count; i++)
            {
                if (ActiveFlares[i].HasFadedOut)
                {
                    ActiveFlares.RemoveAt(i);
                    i--;
                    continue;
                }
                ActiveFlares[i].Update(levelTime, Angle);
            }
        }

        protected virtual void UpdateAngle(float spawnerSpeed)
        {
            angle += spawnerSpeed;
        }

        public override void Draw(DrawBatch drawBatch)
        {
            var hasDrawnFlareCircle = false;
            for (int i = ActiveFlares.Count - 1; i >= 0; i--)
            {
                var flare = ActiveFlares[i];
                if (flare.CurrentColor.A == 255 && !flare.HasFadedOut)
                {
                    Draw(drawBatch, new SolidColorBrush(flare.CurrentColor));
                    hasDrawnFlareCircle = true;
                    break;
                }
            }

            if (!hasDrawnFlareCircle)
            {
                base.Draw(drawBatch);
                indicator.Draw(drawBatch);
            }
        }

        public void DrawFlareCount(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Fonts.Content.Orkney13, ActiveFlares.Count.ToString(),
                Center, Color.Red);
        }

        public void ResetIndicator()
        {
            indicator.Reset();
        }
    }
}