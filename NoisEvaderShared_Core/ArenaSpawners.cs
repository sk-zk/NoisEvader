using LilyPath;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoisEvader
{
    public class ArenaSpawners
    {
        public List<Spawner> Spawners { get; protected set; } = new List<Spawner>();
        protected double[] spawnerAngles;
        private float distBetweenSpawners;

        protected ArenaCircle arena;

        protected PiecewiseFunction combinedSpinWarpFunction;
        private double songPosition;
        protected bool useCenterSpawnerGlitch;

        public ArenaSpawners(ArenaCircle arena, PiecewiseFunction combinedSpinWarpFunction)
        {
            this.arena = arena;
            this.combinedSpinWarpFunction = combinedSpinWarpFunction;
        }

        public void CreateSpawners(SoundodgerLevel level)
        {
            useCenterSpawnerGlitch = level.UsesCenterSpawnerGlitch;
            ConstructSpawners(level);
            SetSpawnerAngles(level.Info.Enemies);
        }

        protected virtual void ConstructSpawners(SoundodgerLevel level)
        {
            var amount = level.Info.Enemies;
            var ceil = (int)Math.Ceiling(amount);
            Spawners = new List<Spawner>(ceil);

            for (int i = 0; i < ceil; i++)
            {
                Spawner spawnerCircle = ConstructSpawner(level);
                SetLastShot(level, i, spawnerCircle);
                Spawners.Add(spawnerCircle);
            }
        }

        protected virtual Spawner ConstructSpawner(SoundodgerLevel level) 
        {
            Spawner spawner;
            if (useCenterSpawnerGlitch)
                spawner = new CenterGlitchSpawner();
            else
                spawner = new Spawner();

            spawner.Arena = arena;
            spawner.BorderColor = level.Info.Colors.Outline;
            spawner.Center = Vector2.Zero;

            return spawner;
        }

        protected virtual void SetSpawnerAngles(double amount)
        {
            // sdgr+ has a, uh, "feature" that lets you set the no. of enemies
            // to a decimal number, giving you ceil(n) enemies.
            // that extra enemy is placed inbetween the previous one and
            // where its actual position would be (if it had been defined with an int).
            // the community uses this for placing two enemies on top of each other,
            // which is a workaround for sdgr+ limitations like not being able to fire
            // more than one stream per enemy.
            // conveniently, this behaviour emerges naturally from the code
            // i'd already written, so I didn't have to change much.
            var ceil = (int)Math.Ceiling(amount);
            spawnerAngles = new double[ceil];
            distBetweenSpawners = (float)(2 * Math.PI / amount);
            for (int i = 0; i < ceil; i++)
            {
                var angle = (i * distBetweenSpawners);
                if (!useCenterSpawnerGlitch)
                {
                    angle -= MathHelper.ToRadians(90);
                }
                Spawners[i].Angle = angle;
                spawnerAngles[i] = angle;
            }
        }

        protected void SetLastShot(SoundodgerLevel level, int i, Spawner spawner)
        {
            var lastShot = level.Script.Shots.LastOrDefault(
                x => x.Enemies.Contains(i));

            if (lastShot is null)
            {
                spawner.DespawnPoint = 0f;
            }
            else
            {
                spawner.DespawnPoint = (float)lastShot.Time;
                if (lastShot is StreamShot ss)
                    spawner.DespawnPoint += (float)ss.Duration;
            }
        }

        public void CreateIndicatorList(SoundodgerLevel level)
        {
            foreach (var shot in level.Script.Shots)
            {
                foreach (var enemyIdx in shot.Enemies)
                {
                    Util.GetSpawnerFromShotIndex(Spawners, enemyIdx)
                        .AddToIndicatorList(
                        shot.Time, shot.Color);
                }
            }
        }

        public void Reset()
        {
            SetSpawnerAngles(Spawners.Count);

            foreach (var spawner in Spawners)
            {
                spawner.ActiveFlares.Clear();
                spawner.ResetIndicator();
            }
        }

        public virtual void Update(LevelTime levelTime)
        {
            songPosition = levelTime.SongPosition; // used in draw call

            /* I'm either insane or insanely brilliant, but it's probably the former.
               The problem: Replays kept desyncing because spawner movement was still
                 framerate-dependent due to accumulating floating point errors
                 (or something like that).
               A normal person would've just locked the update loop to n ticks/s
                 and called it a day, but in MonoGame, as far as I'm aware,
                 that also caps the framerate at n fps, and I didn't want to do that.
               Instead, I decided to calculate the spawner positions independent of
                 game state to sidestep this problem.
               To do this, I turn the SR and TW node lists into piecewise functions,
                 multiply them to get a combined movement function, and then,
                 for each frame, multiply the rotation/ms factor by the integral
                 of that function between 0 and the current song position.
               This appears to have sorted out my issues for the most part. Spawner
                 movement still varies a little bit in moments with high SR*TW values,
                 but since these variances don't accumulate anymore, it always
                 catches itself when the rotation slows down.
            */
            var totalIntegral = combinedSpinWarpFunction.IntegralTo(songPosition);
            for (int i = 0; i < Spawners.Count; i++)
            {
                var spawner = Spawners[i];
                if (spawner.IsStillInUse(levelTime.SongPosition) || spawner.ActiveFlares.Count > 0)
                {
                    spawner.Angle = (float)(spawnerAngles[i] 
                        + MathEx.WrapAngle(totalIntegral * Spawner.RotationPerMs));
                    spawner.Update(levelTime);
                }
            }
        }

        public void Draw(DrawBatch drawBatch)
        {
            foreach (var spawner in Spawners)
            {
                if (spawner.IsStillInUse(songPosition))
                    spawner.Draw(drawBatch);
            }
        }

        public void DrawFlares(DrawBatch drawBatch)
        {
            // draw flares oldest to newest.
            // not a great way of doing it but Maybe We Should looks much better now
            var flares = Spawners.SelectMany(x => x.ActiveFlares).OrderBy(x => x.StartTime);
            foreach (var flare in flares)
                flare.Draw(drawBatch);
        }
    }
}
