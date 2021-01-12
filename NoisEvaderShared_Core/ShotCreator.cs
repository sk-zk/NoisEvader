using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoisEvader
{
    public class ShotCreator
    {
        public SoundodgerLevel Level { get; set; }

        protected Vector2 arenaCenter;
        protected float arenaRadius;
        private CircleParticleSystem particles;
        private List<StreamShot> activeStreams;

        public ShotCreator(Vector2 arenaCenter, float arenaRadius, SoundodgerLevel level,
            List<StreamShot> activeStreams, CircleParticleSystem particles)
        {
            this.arenaRadius = arenaRadius;
            this.arenaCenter = arenaCenter;
            Level = level;
            this.activeStreams = activeStreams;
            this.particles = particles;
        }

        /// <summary>
        /// Creates all waves of a shot.
        /// </summary>
        public List<Bullet> FireShot(Shot shot, List<Spawner> spawners,
             Vector2 playerCenter)
        {
            var newBullets = new List<Bullet>();

            if (shot is StreamShot ss)
            {
                activeStreams.Add(ss);
                foreach (int enemy in ss.Enemies)
                {
                    var spawner = Util.GetSpawnerFromShotIndex(spawners, enemy);
                    spawner.ActiveFlares.Add(new Flare(
                        shot.Color, (float)ss.Duration, arenaCenter, (float)ss.Time));
                }
            }
            else
            {
                foreach (var wave in shot.GetWaves())
                {
                    foreach (var enemy in wave.Enemies)
                    {
                        var spawner = Util.GetSpawnerFromShotIndex(spawners, enemy);
                        newBullets.AddRange(wave.Create(spawner.Center, spawner.Angle, playerCenter, Level.Info.Colors));
                        AddFlare(shot, spawner);

                        if (Config.GameSettings.DrawParticles)
                            particles.CreateShotSpawnParticles(spawner, shot.Color);
                    }
                }
            }

            return newBullets;
        }

        private void AddFlare(Shot shot, Spawner spawner)
        {
            spawner.ActiveFlares.Add(new Flare(shot.Color, 0, arenaCenter, (float)shot.Time));
        }
    }
}
