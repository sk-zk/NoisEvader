using LilyPath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoisEvader
{
    public class BulletManager
    {
        private ShotCreator shotCreator;
        public List<Bullet> Bullets { get; private set; } = new List<Bullet>();

        public List<Shot> Shots { get; private set; }
        private TimeIter<Shot> shotIterator;

        public int UpcomingShots => Shots.Count - shotIterator.CurrentIndex;

        public List<StreamShot> ActiveStreams { get; private set; } = new List<StreamShot>();

        /// <summary>
        /// Bullets that have left the screen, including red bullets.
        /// </summary>
        public int TotalExited { get; private set; } = 0;

        /// <summary>
        /// Bullets that have left the screen without being marked as hit.
        /// </summary>
        public int Score { get; private set; } = 0;

        public float ScorePercent => (TotalExited == 0)
            ? 1
            : (float)Score / TotalExited;

        private ArenaCircle arena;
        private SoundodgerLevel level;
        private List<Spawner> spawners;
        private CircleParticleSystem particles;
        private Mod activeMod;
        private bool slomo;
        private BulletDrawer bulletDrawer;
        private Vector2 playerCenter;

        public BulletManager(ArenaCircle arena, SoundodgerLevel level,
            List<Spawner> spawners, Mod activeMods, CircleParticleSystem particles)
        {
            this.arena = arena;
            shotCreator = new ShotCreator(arena.Circle.Center, arena.Circle.Radius, level, ActiveStreams, particles);
            Shots = new List<Shot>(level.Script.Shots);
            this.spawners = spawners;
            this.activeMod = activeMods;
            this.particles = particles;
            this.level = level;
            bulletDrawer = new BulletDrawer(Bullets);

            shotIterator = new TimeIter<Shot>(Shots, x => x.Time, OnFireShot);
        }

        public CollisionType[] CheckPlayerCollisions(Player player, bool positiveOnly = false)
        {
            var colls = new HashSet<CollisionType>();

            for (int i = 0; i < Bullets.Count; i++)
            {
                Bullet bullet = Bullets[i];

                if (!bullet.Hitbox.Intersects(player.Hitbox))
                    continue;

                switch (bullet)
                {
                    case Heart _:
                        player.HasHeart = true;
                        bullet.Hit = HitType.Normal;
                        colls.Add(CollisionType.Heart);
                        break;

                    case Hug hug:
                        hug.PlayerHugged();
                        colls.Add(CollisionType.Hug);
                        break;

                    default:
                        if (player.Invincibility != InvincibilityType.None || positiveOnly)
                            continue;

                        bullet.Hit = player.HasHeart ? HitType.Heart : HitType.Normal;
                        colls.Add(CollisionType.Bad);
                        break;
                }
            }

            if (!player.HasHeart && colls.Contains(CollisionType.Bad))
                MarkAllBulletsHit();

            return colls.ToArray();
        }

        /// <summary>
        /// Spawns, despawns and updates bullets and starts indicator animations.
        /// </summary>
        public CollisionType Update(LevelTime levelTime, Player player, 
            CircleParticleSystem particles, bool slomo)
        {
            playerCenter = player.Center;

            var collType = CollisionType.None;

            this.slomo = slomo;

            // update bullets on screen
            for (int i = 0; i < Bullets.Count; i++)
            {
                var bullet = Bullets[i];

                MoveBullet(levelTime, bullet, player.Center);

                (bool left, bool forgotHug) = CheckIfBulletLeftArena(bullet, slomo);
                if (left)
                {
                    if (Config.GameSettings.DrawParticles)
                        particles.CreateBulletDespawnParticles(bullet);
                    Bullets.RemoveAt(i);
                    i--;
                }
                if (forgotHug && player.Invincibility == InvincibilityType.None)
                {
                    collType = CollisionType.Bad;
                }
            }

            if (!player.HasHeart && collType == CollisionType.Bad)
                MarkAllBulletsHit();

            shotIterator.Update(levelTime);
            UpdateStreams(levelTime, player.Center);

            return collType;
        }

        private (bool left, bool forgotHug) CheckIfBulletLeftArena(Bullet bullet, bool slomo)
        {
            var forgotHug = false;
            var left = false;
            if (bullet.DespawnNow(arena.Hitbox))
            {
                left = true;

                if (bullet is Hug hug)
                {
                    if (hug.IsHugged)
                    {
                        if (!slomo)
                            Score++;
                    }
                    else if (activeMod.Flags.HasFlag(ModFlags.Showcase))
                    {
                        Score++;
                    }
                    else
                    {
                        // player forgot a hug
                        forgotHug = true;
                    }
                }
                else if (ShouldIncreaseScore(bullet))
                {
                    Score++;
                }

                if (ShouldIncreaseTotalBullets(bullet))
                    TotalExited++;
            }
            return (left, forgotHug);
        }

        private static void MoveBullet(LevelTime levelTime, Bullet bullet, Vector2 playerCenter)
        {
            if (bullet is Homing h)
                h.Update(levelTime, playerCenter);
            else
                bullet.Update(levelTime);
        }

        /// <summary>
        /// Determines if a bullet about to despawn should be counted towards score.
        /// </summary>
        private bool ShouldIncreaseScore(Bullet bullet) =>
            !(bullet.Hit == HitType.Normal || slomo || bullet is Heart);

        private bool ShouldIncreaseTotalBullets(Bullet bullet) =>
            !(bullet is Heart);

        private void OnFireShot(TimeIterActionArgs<Shot> args)
        {
            var newBullets = shotCreator.FireShot(args.List[args.Index], spawners, playerCenter);
            Bullets.AddRange(newBullets);
        }

        /// <summary>
        /// Updates streams.
        /// </summary>
        private void UpdateStreams(LevelTime levelTime, Vector2 playerCenter)
        {
            // TODO Refactor this old mess

            if (ActiveStreams.Count == 0)
                return;

            for (int i = 0; i < ActiveStreams.Count; i++)
            {
                var stream = ActiveStreams[i];

                stream.TimeSinceLastShot += (float)levelTime.ElapsedMs;
                if (stream.TimeSinceLastShot >= StreamShot.FireFrequency)
                {
                    var currStreamTime = levelTime.SongPosition - stream.Time;
                    var currSpeed = stream.Speed0 + (currStreamTime * ((stream.Speed1 - stream.Speed0) / stream.Duration));
                    var currAngle = stream.Angle0 + (currStreamTime * ((stream.Angle1 - stream.Angle0) / stream.Duration));
                    currAngle = MathEx.WrapAngle(currAngle);
                    var currOffset = stream.Offset0 + (currStreamTime * ((stream.Offset1 - stream.Offset0) / stream.Duration));
                    currOffset = MathEx.WrapAngle(currOffset);

                    bool lastShot = levelTime.SongPosition + StreamShot.FireFrequency > stream.Time + stream.Duration;

                    foreach (int enemy in stream.Enemies)
                    {
                        var spawner = Util.GetSpawnerFromShotIndex(spawners, enemy);
                        var wave = new Wave()
                        {
                            BulletType = stream.BulletType,
                            Amount = stream.Amount0,
                            Speed = currSpeed,
                            Angle = currAngle,
                            Offset = currOffset,
                            Aim = stream.Aim,
                            HomingLifespan = stream.HomingLifespan,
                        };
                        Bullets.AddRange(wave.Create(spawner.Center, spawner.Angle,
                            playerCenter, level.Info.Colors));

                        if (lastShot && Config.GameSettings.DrawParticles)
                            particles.CreateShotSpawnParticles(spawner, stream.Color);
                    }

                    if (lastShot)
                    {
                        ActiveStreams.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        stream.TimeSinceLastShot -= StreamShot.FireFrequency;
                    }
                }
            }
        }

        /// <summary>
        /// Marks all bullets currently on screen as hit.
        /// </summary>
        public void MarkAllBulletsHit()
        {
            for (int i = 0; i < Bullets.Count; i++)
            {
                if (!(Bullets[i] is Heart))
                    Bullets[i].Hit = HitType.Normal;
            }
        }

        public void Reset()
        {
            shotIterator.Reset();
            Bullets.Clear();
            ActiveStreams.Clear();
            Score = 0;
            TotalExited = 0;
        }

        public void Draw(SpriteBatch spriteBatch, DrawBatch drawBatch)
        {
            bulletDrawer.Draw(spriteBatch, drawBatch);
        }
    }

    public enum CollisionType
    {
        None,
        Hug,
        Heart,
        Bad,
    }
}
