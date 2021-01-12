using LilyPath;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    public interface IHitbox
    {
        /// <summary>
        /// Returns if a point is inside the hitbox.
        /// </summary>
        bool IsPointInside(Vector2 point);

        bool Intersects(IHitbox hitbox);

        void Draw(DrawBatch drawBatch);

    }
}
