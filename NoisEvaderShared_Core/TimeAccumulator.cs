using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    public class TimeAccumulator
    {
        public Action<GameTime> Tick { get; set; }

        public double Step { get; set; }

        private double elapsed;

        public TimeAccumulator(double step)
        {
            Step = step;
        }

        public void Update(GameTime gameTime)
        {
            elapsed += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (elapsed > Step)
            {
                var lateBy = elapsed - Step;
                elapsed -= Step;
                var tickGt = new GameTime(gameTime.TotalGameTime, TimeSpan.FromMilliseconds(Step));
                Tick?.Invoke(gameTime);
            }
        }
    }
}
