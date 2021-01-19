using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoisEvader
{
    public class Tweener
    {
        /// <summary>
        /// Determines if the tweener is currently updating.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Starting value.
        /// </summary>
        public float Start { get; private set; }

        /// <summary>
        /// End value.
        /// </summary>
        public float End { get; private set; }

        /// <summary>
        /// The duration of the tween.
        /// </summary>
        public float Duration { get; private set; }

        /// <summary>
        /// Current value.
        /// </summary>
        public float CurrentValue { get; private set; }

        /// <summary>
        /// Progress in percent.
        /// </summary>
        public float Progress => MathHelper.Clamp(Elapsed / Duration, 0, 1);

        /// <summary>
        /// How much time has passed since the tween started.
        /// </summary>
        public float Elapsed { get; private set; }

        /// <summary>
        /// The relative interpolation / easing function to use.
        /// </summary>
        public Func<float, float> Interpolation { get; set; }

        /// <summary>
        /// Whether or not the tween repeats.
        /// </summary>
        public bool Repeat { get; set; }

        /// <summary>
        /// Delay between repeats.
        /// </summary>
        public float RepeatDelay { get; set; }

        /// <summary>
        /// Creates a new tweener with the given interpolation function.
        /// </summary>
        /// <param name="interpolation">The relative interpolation / easing function to use.</param>
        public Tweener(Func<float, float> interpolation)
        {
            Interpolation = interpolation;
        }

        /// <summary>
        /// Starts a tween.
        /// </summary>
        /// <param name="startVal">Starting value.</param>
        /// <param name="endVal">End value.</param>
        /// <param name="duration">Duration in ms.</param>
        public void Animate(float startVal, float endVal, float duration,
            float startDelay = 0, bool repeat = false, float repeatDelay = 0)
        {
            Start = startVal;
            End = endVal;
            Duration = duration;
            Elapsed = -startDelay;
            CurrentValue = startVal;
            Repeat = repeat;
            RepeatDelay = repeatDelay;
            Enabled = true;
        }

        public void Animate(float startVal, float endVal, float duration)
        {
            Animate(startVal, endVal, duration, 0, false, 0);
        }

        public void Update(float elapsedMs)
        {
            if (!Enabled)
                return;

            Elapsed += elapsedMs;

            // some quality code right here
            if (Elapsed >= Duration)
            {
                if (Repeat)
                {
                    if (RepeatDelay > 0)
                    {
                        if (Elapsed >= Duration + RepeatDelay)
                        {
                            Elapsed = Elapsed - Duration - RepeatDelay;
                        }
                        else
                        {
                            CurrentValue = End;
                            return;
                        }
                    }
                    else
                    {
                        Elapsed -= Duration;
                    }
                }
                else
                {
                    Enabled = false;
                    CurrentValue = End;
                    return;
                }
            }
            else if (Elapsed < 0)
            {
                if (Repeat)
                {
                    if (RepeatDelay > 0)
                    {
                        if (Elapsed <= (-RepeatDelay))
                        {
                            Elapsed = Elapsed + Duration + RepeatDelay;
                        }
                        else
                        {
                            CurrentValue = Start;
                            return;
                        }
                    }
                    else
                    {
                        Elapsed += Duration;
                    }
                }
                else
                {
                    CurrentValue = Start;
                    return;
                }
            }

            var progress = Interpolation(Progress);
            CurrentValue = Start + (progress * (End - Start));
        }

    }
}
