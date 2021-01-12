using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoisEvader
{
    public static class MathEx
    {
        /// <summary>
        /// Square root of 2.
        /// </summary>
        public const float Sqrt2 = 1.41421353816986083984375f;

        /// <summary>
        /// Half the square root of 2.
        /// </summary>
        public const float HalfSqrt2 = Sqrt2 / 2;

        /// <summary>
        /// Factor for converting radians to degrees.
        /// </summary>
        public const double RadToDeg = 180.0 / Math.PI;

        /// <summary>
        /// Factor for converting degrees to radians.
        /// </summary>
        public const double DegToRad = Math.PI / 180.0;

        /// <summary>
        /// Checks if a circle is at least partially contained in another.
        /// </summary>
        public static bool IsCircleInCircle(Vector2 center1, float radius1, Vector2 center2, float radius2)
        {
            var distanceX = center1.X - center2.X;
            var distanceY = center1.Y - center2.Y;
            var radiusSum = radius1 + radius2;
            return (distanceX * distanceX) + (distanceY * distanceY) <= radiusSum * radiusSum;
        }

        public static float EaseInOutSine(float n) =>
            (float)(0.5 * (1 - Math.Cos(Math.PI * n)));

        /// <summary>
        /// Calculates a point on the circumference of a circle.
        /// </summary>
        public static Vector2 PointOnCircumference(Vector2 center, float radius, float angle)
        {
            var x = (float)(center.X + radius * Math.Cos(angle));
            var y = (float)(center.Y + radius * Math.Sin(angle));
            return new Vector2(x, y);
        }

        public static Vector2 GetBezierPoint(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            // via https://github.com/sqrMin1/MonoGame.SplineFlower/blob/master/MonoGame.SplineFlower/Bezier.cs
            // MIT license

            t = MathHelper.Clamp(t, 0f, 1f);
            float oneMinusT = 1f - t;
            return
                (oneMinusT * oneMinusT * oneMinusT * p0) +
                (3f * oneMinusT * oneMinusT * t * p1) +
                (3f * oneMinusT * t * t * p2) +
                (t * t * t * p3);
        }

        public static Vector2[] GetBezierPoints(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, int steps)
        {
            var points = new Vector2[steps];
            for (int step = 0; step < steps; step++)
                points[step] = GetBezierPoint(p0, p1, p2, p3, (float)step / (steps - 1));
            return points;
        }

        public static double WrapAngle(double angle)
        {
            angle = Math.IEEERemainder(angle, 6.2831854820251465);
            if (angle <= -3.1415926535897932)
            {
                angle += 6.2831854820251465;
            }
            else
            {
                if (angle > 3.1415926535897932)
                    angle -= 6.2831854820251465;
            }
            return angle;
        }

        public static Vector2 MouseToWorld(Vector2 mouse, Vector2 center, float scale)
        {
            var screen = mouse;
            screen.X -= NoisEvader.ScreenBounds.Width / 2;
            screen.Y -= NoisEvader.ScreenBounds.Height / 2;
            screen -= center;
            screen /= scale;
            screen += center;
            return screen;
        }
    }
}
