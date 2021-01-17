using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LilyPath;
using Microsoft.Xna.Framework;

namespace NoisEvader
{
    public class LiveSpawner : Spawner
    {
        // TODO: Unfuck TransIn

        public float ActiveZone { get; set; } = MathHelper.ToRadians(360);
        public float TransitionZone { get; set; } = 0;

        public LiveSpawner() : base() { }

        public LiveSpawner(float angle, ArenaCircle arena)
            : base(angle, arena) { }

        protected override void SetAngle(float value)
        {
            angle = value;
            WrapAngle();
            SkipPastInactiveZone();
            CalcPosition(Arena);
        }

        private void SkipPastInactiveZone()
        {
            if (ActiveZone >= MathHelper.ToRadians(360))
                return;

            var minArc = MathHelper.ToRadians(-90) - (ActiveZone / 2);
            var maxArc = MathHelper.ToRadians(-90) + (ActiveZone / 2);
            if (angle > maxArc)
            {
                angle -= ActiveZone;
            }
            else if (angle < minArc)
            {
                angle += ActiveZone;
            }
        }

        public override void Update(LevelTime levelTime)
        {
            base.Update(levelTime);
        }

        public override void Draw(DrawBatch drawBatch)
        {
            base.Draw(drawBatch);
        }

    }
}
