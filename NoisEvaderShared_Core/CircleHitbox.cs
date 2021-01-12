using LilyPath;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    public class CircleHitbox : IHitbox
    {
        /// <summary>
        /// If true, the outside of the circle is the collider
        /// rather than the inside.
        /// </summary>
        public bool Inverted { get; set; }

        public Circle Circle { get; private set; }

        public CircleHitbox(Circle circle)
        {
            Circle = circle;
        }

        public bool IsPointInside(Vector2 point)
        {
            var collides = Circle.IsPointInCircle(point);
            if (Inverted)
                collides = !collides;
            return collides;
        }

        public bool Intersects(IHitbox hitbox)
        {
            if (hitbox is null)
                return false;

            if (hitbox is CircleHitbox c)
            {
                return Circle.IsCircleInCircle(c.Circle);
            }
            else if (hitbox is PointHitbox p)
            {
                return IsPointInside(p.Point);
            }
            throw new NotImplementedException();
        }

        public void Draw(DrawBatch drawBatch)
        {
            drawBatch.FillCircle(new SolidColorBrush(new Color(Color.Red, 0.8f)),
                Circle.Center, Circle.Radius);
        }
    }
}
