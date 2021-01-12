using LilyPath;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    public class PointHitbox : IHitbox
    {
        public Vector2 Point { get; set; }

        public bool Intersects(IHitbox hitbox)
        {
            if (hitbox is null)
                return false;

            if (hitbox is CircleHitbox c)
            {
                return c.IsPointInside(Point);
            }
            else if(hitbox is PointHitbox p)
            {
                return IsPointInside(p.Point);
            }
            throw new NotImplementedException();
        }

        public bool IsPointInside(Vector2 point) =>
            point == Point;

        public void Draw(DrawBatch drawBatch)
        {
            drawBatch.DrawPoint(new Pen(Color.Red, 2), Point);
        }
    }
}
