using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    public struct Wave
    {
        public int[] Enemies { get; set; }
        public BulletType BulletType { get; set; }
        public int Amount { get; set; }
        public double Speed { get; set; }
        public double Angle { get; set; }
        public double Offset { get; set; }
        public Aim Aim { get; set; }
        public double HomingLifespan { get; set; }

        /// <summary>
        /// Creates one wave of a shot.
        /// </summary>
        public List<Bullet> Create(Vector2 spawnPoint, float spawnerAngle, Vector2 playerCenter,
            LevelColors colors)
        {
            var bullets = new List<Bullet>(Amount);

            double waveAngle = spawnerAngle + (90 * MathEx.DegToRad);
            if (Amount > 1)
                waveAngle += Angle / -2;

            if (Aim == Aim.Player)
            {
                float playerAngle = (float)Math.Atan2(
                    spawnPoint.Y - playerCenter.Y,
                    spawnPoint.X - playerCenter.X)
                    - spawnerAngle;
                waveAngle += playerAngle;
            }

            MathEx.WrapAngle(waveAngle);

            var angleStep = Angle / (Amount - 1);
            for (int i = 0; i < Amount; i++)
            {
                var bulletAngle = waveAngle + Offset;

                if (BulletType == BulletType.Homing)
                {
                    bullets.Add(CreateHomingBullet(spawnPoint, (float)bulletAngle,
                        (float)Speed, (float)HomingLifespan, colors));
                }
                else
                {
                    bullets.Add(CreateBullet(BulletType, spawnPoint, (float)bulletAngle,
                        (float)Speed, colors));
                }
                waveAngle += angleStep;
            }

            return bullets;
        }

        /// <summary>
        /// Adds a single bullet to the screen; should only be called from CreateOneWave.
        /// </summary>
        private Bullet CreateBullet(BulletType bulletType, Vector2 spawnPoint, float angle,
            float speed, LevelColors colors)
        {
            switch (bulletType)
            {
                case BulletType.Normal:
                    return new Spike(spawnPoint, speed, angle, colors.Normal);
                case BulletType.Normal2:
                    return new Spike(spawnPoint, speed, angle, colors.Normal2);
                case BulletType.Homing:
                    throw new ArgumentException("Spawn homing bullets with CreateHomingBullet please");
                case BulletType.Bubble:
                    return new Bubble(spawnPoint, speed, angle, colors.Bubble);
                case BulletType.Heart:
                    return new Heart(spawnPoint, speed, angle);
                case BulletType.Hug:
                    return new Hug(spawnPoint, speed, angle, colors.Hug);
                case BulletType.Error:
                    return new ErrorBullet(spawnPoint, speed, angle);
                default:
                    return new Spike(spawnPoint, speed, angle, colors.Normal);
            }
        }

        private Bullet CreateHomingBullet(Vector2 spawnPoint, float angle, float speed,
            float lifespan, LevelColors colors)
        {
            return new Homing(spawnPoint, speed, angle, lifespan, colors.Homing);
        }
    }
}
