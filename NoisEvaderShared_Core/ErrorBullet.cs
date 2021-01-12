using LilyPath;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    /// <summary>
    /// Recreates the "error bullet" of soundodger+.
    /// </summary>
    public class ErrorBullet : Bubble
    {
        public override HitType Hit
        {
            get => hit;
            set
            {
                hit = value;
                spike.Hit = value;
            }
        }

        private ErrorHoming homing;
        private Spike spike;

        public ErrorBullet(Vector2 startingPosition, float speed, float rotation)
            : base(startingPosition, speed, rotation, Color.Red)
        {
            glow.Animation.RepeatDelay = 0;
            homing = new ErrorHoming(startingPosition, speed, rotation);
            spike = new Spike(startingPosition, speed, rotation, new Color(0, 16, 52));
            spike.DrawGlow = false;
        }

        public override bool IsOutsideCircle(CircleHitbox circle) =>
            spike.IsOutsideCircle(circle);

        public override void Update(LevelTime levelTime)
        {
            base.Update(levelTime);
            homing.Update(levelTime, Vector2.Zero);
            spike.Update(levelTime);
        }

        public override void Draw(DrawBatch batch)
        {
            spike.Draw(batch);
            homing.Draw(batch);
            glow.Draw(batch, Color.White);
        }
    }
}
