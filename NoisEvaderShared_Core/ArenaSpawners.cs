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

        protected ArenaCircle arena;

        private float distBetweenSpawners;
        private double songPosition;

        public ArenaSpawners(ArenaCircle arena)
        {
            this.arena = arena;
        }

        public void CreateSpawners(SoundodgerLevel level)
        {
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
            return new Spawner()
            {
                Arena = arena,
                BorderColor = level.Info.Colors.Outline,
            };
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
            distBetweenSpawners = (float)(2 * Math.PI / amount);
            for (int i = 0; i < ceil; i++)
            {
                Spawners[i].Angle = (i * distBetweenSpawners) - MathHelper.ToRadians(90);
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
            var spawnerSpeed = CalcSpawnerSpeed(levelTime);
            foreach (var spawner in Spawners)
            {
                if (spawner.IsStillInUse(levelTime.SongPosition) || spawner.ActiveFlares.Count > 0)
                    spawner.Update(levelTime, (float)spawnerSpeed);
            }
        }

        private double CalcSpawnerSpeed(LevelTime levelTime)
        {
            return levelTime.LevelElapsedMs
                * levelTime.SpinRate
                * Spawner.RotationPerMs;
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
