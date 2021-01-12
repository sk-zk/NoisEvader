using LilyPath;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    public class ErrorHoming : Homing
    {
        public ErrorHoming(Vector2 startingPosition, float speed, float rotation)
            : base(startingPosition, speed, rotation, 0, Color.White)
        {
            glow.Animation.RepeatDelay = 0;
        }

        public override void Draw(DrawBatch batch)
        {
            glow.Draw(batch, this, Color.White);
        }
    }
}
