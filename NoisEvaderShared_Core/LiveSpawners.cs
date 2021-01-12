using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    public class LiveSpawners : ArenaSpawners
    {
        private readonly float liveModSpawnerZone = MathHelper.ToRadians(180);
        private readonly float liveModTransitionZone = MathHelper.ToRadians(14);

        public LiveSpawners(ArenaCircle arena) : base(arena) { }

        protected override Spawner ConstructSpawner(SoundodgerLevel level)
        {
            return new LiveSpawner()
            {
                Arena = arena,
                BorderColor = level.Info.Colors.Outline,
            };
        }

        protected override void SetSpawnerAngles(double amount)
        {
            var ceil = (int)Math.Ceiling(amount);

            float angle;
            float distBetweenSpawners;
            float startAngle;

            angle = liveModSpawnerZone;
            distBetweenSpawners = (float)(angle / amount);
            startAngle = MathHelper.ToRadians(90) + angle / 2f
                - distBetweenSpawners / 2f;
            for (int i = 0; i < ceil; i++)
            {
                var spawner = (LiveSpawner)Spawners[i];
                spawner.Angle = i * distBetweenSpawners - startAngle;
                spawner.ActiveZone = angle;
                spawner.TransitionZone = liveModTransitionZone;
            }
        }

    }
}
