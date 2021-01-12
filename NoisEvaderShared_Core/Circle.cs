using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    public class Circle
    {
        protected Vector2 position = new Vector2(0, 0);
        public virtual Vector2 Position
        {
            get => position;
            set => position = value;
        }

        public virtual float Radius { get; set; } = 10f;

        public virtual Vector2 Center
        {
            get
            {
                Vector2 center = position;
                center.X += Radius;
                center.Y += Radius;
                return center;
            }
            set
            {
                position.X = value.X - Radius;
                position.Y = value.Y - Radius;
            }
        }

        /// <summary>
        /// Checks if a point is contained in this circle.
        /// </summary>
        public bool IsPointInCircle(Vector2 point)
        {
            var center = Center;
            Vector2.Distance(ref center, ref point, out float distance);
            return distance < Radius;
        }

        /// <summary>
        /// Checks if a circle is at least partially contained in another.
        /// </summary>
        public bool IsCircleInCircle(Circle circle2) =>
            MathEx.IsCircleInCircle(Center, Radius, circle2.Center, circle2.Radius);

        /// <summary>
        /// Calculates a point on the circumference of the circle.
        /// </summary>
        public Vector2 PointOnCircumference(float angle) =>
            MathEx.PointOnCircumference(Center, Radius, angle);

    }
}
