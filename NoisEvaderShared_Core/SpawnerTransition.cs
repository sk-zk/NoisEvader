using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoisEvader
{
    public class SpawnerTransition : Spawner
    {
        private float startRadius;
        private float endRadius;
        private float startAngle;
        private float endAngle;

        public SpawnerTransition(float angle, ArenaCircle arena,
            float startAngle, float endAngle, float startRadius, float endRadius)
            : base(angle, arena)
        {
            Radius = startRadius;
            this.startAngle = startAngle;
            this.endAngle = endAngle;
            this.startRadius = startRadius;
            this.endRadius = endRadius;
        }

        public override void Update(LevelTime levelTime)
        {
            base.Update(levelTime);

            var progress = (Angle - startAngle) / (endAngle - startAngle);
            progress = MathHelper.Clamp(progress, 0, 1);
            progress = MathEx.EaseInOutSine(progress);
            Radius = MathHelper.Lerp(startRadius, endRadius, progress);
        }

    }
}
