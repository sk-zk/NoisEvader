using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    public abstract class Glow
    {
        public Tweener Animation { get; private set; }

        public Color BaseColor { get; set; } = Color.White;

        public Glow()
        {
            Animation = new Tweener((x) => x);
        }

        public void Update(LevelTime levelTime)
        {
            Animation.Update((float)levelTime.LevelElapsedMs);
        }
    }
}
