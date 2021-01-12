using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace NoisEvader
{
    /// <summary>
    /// Base class for bullets with trails (Homing and Heart).
    /// </summary>
    public abstract class TrailBullet : Bullet
    {
        /// <summary>
        /// Particles for this bullet. The base class does not
        /// update or draw it automatically.
        /// </summary>
        protected CircleParticleSystem particles;
        protected CircleParticleSystem.CircularParameters particleParameters;
        protected float particlesInterval = 100;
        protected float particlesElapsed = 0;
        protected bool spawnParticles;

        protected TrailBullet(Vector2 startingPosition, float speed, float rotation, Color color)
            : base(startingPosition, speed, rotation, color)
        {
            particles = new CircleParticleSystem();
            particleParameters = new CircleParticleSystem.CircularParameters()
            {
                SystemCenter = Vector2.Zero,
                SystemRadius = (5, 5),
                Amount = 5,
                Color = Color,
                Radius = (0.305f, 1.9f),
                Speed = (BaseSpeed * 0.006f, BaseSpeed * 0.006f),
                ShrinkSpeed = (11.35f, 16.4f),
                Angle = (0, MathHelper.TwoPi),
            };
        }

        protected virtual void UpdateParticles(LevelTime levelTime)
        {
            var elapsed = (float)levelTime.LevelElapsedMs;
            particles.Update(levelTime);
            if (spawnParticles)
            {
                particlesElapsed += elapsed;
                if (particlesElapsed >= particlesInterval)
                {
                    CreateParticles();
                    particlesElapsed = 0;
                }
            }
        }

        protected virtual void CreateParticles()
        {
            if (Config.GameSettings.DrawParticles)
                particles.SpawnCircular(particleParameters);
        }
    }
}
