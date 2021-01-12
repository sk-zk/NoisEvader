using LilyPath;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoisEvader
{
    public class CircleParticleSystem
    {
        public int Count => particles.Count;

        private const int MaxParticles = 1024;
        private List<CircleParticle> particles = new List<CircleParticle>(MaxParticles);
        private readonly Random random = new Random();

        public void CreatePlayerHitParticles(Vector2 center)
        {
            const float systemRadius = 3.15f;
            const float minRadius = 4.4f;
            const float maxRadius = 8.2f;
            const float shrinkSpeed = 12.6f;
            SpawnCircular(new CircularParameters()
            {
                SystemCenter = center,
                SystemRadius = (systemRadius, systemRadius),
                Amount = 5,
                Radius = (minRadius, maxRadius),
                ShrinkSpeed = (shrinkSpeed, shrinkSpeed),
                Speed = (0.09f, 0.09f),
                Angle = (0, MathHelper.TwoPi),
                Color = Color.Red,
                Subdivisions = 16,
            });
        }

        public void CreateShotSpawnParticles(Spawner spawner, Color color)
        {
            var radius = spawner.Radius / 2;
            SpawnCircular(new CircularParameters()
            {
                SystemCenter = spawner.Center,
                SystemRadius = (-5, 5),
                Radius = (radius, radius),
                Amount = (uint)random.Next(5, 9),
                ShrinkSpeed = (26f, 32f),
                Speed = (0.045f, 0.05f),
                Angle = (0, MathHelper.TwoPi),
                Color = color,
                Subdivisions = 16,
            });
        }

        public void CreateBulletDespawnParticles(Bullet bullet)
        {
            const float minRadius = 3.8f;
            const float maxRadius = 6.3f;
            const float minShrinkSpeed = 15.75f;
            const float maxShrinkSpeed = 18.9f;
            var p = new CircularParameters()
            {
                Amount = 5,
                SystemCenter = bullet.Position,
                SystemRadius = (0, 0),
                Radius = (minRadius, maxRadius),
                Color = bullet.Color,
                ShrinkSpeed = (minShrinkSpeed, maxShrinkSpeed),
                Speed = (46 / 1000f, 55 / 1000f),
                Angle = (0, MathHelper.TwoPi),
                Inertia = bullet.Velocity / 4,
                Subdivisions = 12,
            };

            switch (bullet)
            {
                case Spike spike:
                    p.SystemCenter = spike.Position;
                    break;
                case Bubble _:
                    p.Radius.Min = 6.3f;
                    p.Radius.Max = 9.45f;
                    break;
                case Homing homing:
                    p.SystemCenter = homing.Tip;
                    break;
                case Hug _:
                    p.Radius.Min = 6.3f;
                    p.Radius.Max = 9.45f;
                    break;
            }

            SpawnCircular(p);
        }

        /// <summary>
        /// Spawns particles in a circular area.
        /// </summary>
        public void SpawnCircular(CircularParameters p)
        {
            for (var i = 0; i < p.Amount; i++)
            {
                var angle = random.Next(p.Angle.Min, p.Angle.Max);
                var position = GetRandomPosition(p.SystemCenter,p.SystemRadius.Min, p.SystemRadius.Max, (float)angle);
                var radius = random.Next(p.Radius.Min, p.Radius.Max);
                var shrinkSpeed = random.Next(p.ShrinkSpeed.Min, p.ShrinkSpeed.Max);
                var speed = random.Next(p.Speed.Min, p.Speed.Max);
                var velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (float)speed;

                particles.Add(new CircleParticle
                {
                    Radius = (float)radius,
                    LineThickness = 0,
                    Center = position,
                    Subdivisions = p.Subdivisions,
                    FillColor = p.Color,
                    Velocity = velocity + p.Inertia,
                    ShrinkSpeed = (float)shrinkSpeed,
                });
            }

            var diff = particles.Count - MaxParticles;
            if (diff > 0)
            {
                particles.RemoveRange(0, diff);
            }
        }

        private Vector2 GetRandomPosition(Vector2 center, float minRadius, float maxRadius, float angle)
        {
            var radius = (float)random.Next(minRadius, maxRadius);
            return MathEx.PointOnCircumference(center, radius, angle);
        }

        /// <summary>
        /// Deletes all particles.
        /// </summary>
        public void Clear()
        {
            particles.Clear();
        }

        public void Update(LevelTime levelTime)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].Update(levelTime);

                if (particles[i].Radius == 0f)
                {
                    particles.RemoveAt(i);
                    i--;
                }
            }
        }

        public void Draw(DrawBatch drawBatch)
        {
            for (int i = 0; i < particles.Count; i++)
                particles[i].Draw(drawBatch);
        }

        public class CircularParameters
        {
            public uint Amount;
            public Vector2 SystemCenter;
            public (float Min, float Max) SystemRadius;
            public (float Min, float Max) Angle;
            public (float Min, float Max) Radius;
            public (float Min, float Max) Speed;
            public (float Min, float Max) ShrinkSpeed;
            public Vector2 Inertia = Vector2.Zero;
            public Color Color;
            public int Subdivisions = 16;
        }
    }

}
