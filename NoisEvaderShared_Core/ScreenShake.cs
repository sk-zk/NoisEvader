using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoisEvader
{
    /// <summary>
    /// Generates screen offset values for a screenshake effect.
    /// </summary>
    public class ScreenShake
    {
        public float Magnitude { get; private set; }

        public float Duration { get; private set; }

        public Vector3 Offset { get; private set; }

        private float currentPos;

        private bool enabled;

        private readonly Random random;

        public ScreenShake()
        {
            random = new Random();
        }

        public void Shake(float magnitude, float duration)
        {
            Magnitude = magnitude;
            Duration = duration;
            currentPos = 0;
            Offset = Vector3.Zero;
            enabled = true;
        }

        public void Update(LevelTime levelTime)
        {
            if (!enabled)
                return;

            var elapsed = (float)levelTime.ElapsedMs;
            currentPos += elapsed;

            if (currentPos >= Duration)
            {
                enabled = false;
                currentPos = Duration;
            }

            var progress = currentPos / Duration;
            var magnitude = Magnitude * (1f - (progress * progress));

            Offset = new Vector3(GetRandomFloat(), GetRandomFloat(), 0) * magnitude;
        }

        private float GetRandomFloat() =>
            (float)(random.NextDouble() * 2 - 1);
    }
}
