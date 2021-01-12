using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoisEvader
{
    public class FpsCounter
    {
        private double internalFps;
        public double Fps { get; private set; }

        private double internalAverageFps;
        public double AverageFps { get; private set; }

        private double sinceUpdate;
        /// <summary>
        /// Sets the rate at which the public members are updated.
        /// This does not affect the internal values.
        /// </summary>
        public double UpdateStep { get; set; } = 1000.0 / 30.0;

        private const int QueueSize = 25;
        private readonly Queue<double> samples = new Queue<double>(QueueSize);

        public void Update(GameTime gameTime)
        {
            internalFps = 1 / gameTime.ElapsedGameTime.TotalSeconds;

            samples.Enqueue(internalFps);
            if (samples.Count > QueueSize)
            {
                samples.Dequeue();
                internalAverageFps = samples.Average(x => x);
            }
            else
            {
                internalAverageFps = internalFps;
            }

            UpdatePublicValues(gameTime);
        }

        private void UpdatePublicValues(GameTime gameTime)
        {
            if (UpdateStep > 0)
            {
                sinceUpdate += gameTime.ElapsedGameTime.TotalMilliseconds;

                if (sinceUpdate >= UpdateStep)
                    sinceUpdate -= UpdateStep;
                else
                    return;
            }
            Fps = internalFps;
            AverageFps = internalAverageFps;
        }

        public void Flush()
        {
            samples.Clear();
        }
    }
}
