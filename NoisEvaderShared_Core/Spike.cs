using LilyPath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoisEvader
{
    public class Spike : Bullet, IDrawable
    {
        public static readonly float SpikeWidth = 20f; // shorter side
        public static readonly float SpikeHeight = 25f; // longer side

        public Vector2 Tip => new Vector2(
            position.X - ((float)Math.Sin(BaseRotation) * (SpikeHeight * 0.5f)),
            position.Y - ((float)Math.Cos(BaseRotation) * (SpikeHeight * -0.5f)));

        // Shape of the spike. The first point is the tip, the rest is the curve.
        private static List<Vector2> shape = new List<Vector2>();
        private static int[] tris;

        private SpikeGlow glow;

        static Spike()
        {
            GenerateShape();
        }

        public Spike(Vector2 startingPosition, float speed, float rotation, Color color)
            : base(startingPosition, speed, rotation, color)
        {
            rotationOrigin = shape[0];
            glow = new SpikeGlow(BaseSpeed);
            Hitbox = new PointHitbox()
            {
                Point = startingPosition,
            };
        }

        private static void GenerateShape()
        {
            var tip = new Vector2(0, 0.5f);
            shape.Add(tip);

            var start = new Vector2(-0.3828081f, -0.4251375f);
            var ctrl1 = new Vector2(-0.1419376f, -0.5258031f);
            var ctrl2 = new Vector2(0.142546f, -0.5256348f);
            var end = new Vector2(0.3828081f, -0.4251375f);

            const int subdivs = 15;
            shape.AddRange(MathEx.GetBezierPoints(start, ctrl1, ctrl2, end, subdivs));

            var transform = Matrix.CreateScale(SpikeHeight);
            for (int i = 0; i < shape.Count; i++)
            {
                shape[i] = Vector2.Transform(shape[i], transform);
            }

            // triangulate with triangle fan.
            var faceAmt = subdivs - 1;
            var triIdx = 0;
            tris = new int[faceAmt * 3];
            for (int i = 1; i < faceAmt; i++)
            {
                tris[triIdx++] = 0;
                tris[triIdx++] = i;
                tris[triIdx++] = i + 1;
            }
        }

        public override bool IsOutsideCircle(CircleHitbox circle) =>
            !circle.IsPointInside(Position);

        public override bool DespawnNow(CircleHitbox circle) =>
            Age > MinimumAge && !circle.IsPointInside(Position);

        public override void Update(LevelTime levelTime)
        {
            (Hitbox as PointHitbox).Point = Tip;
            base.Update(levelTime);
            glow.Update(levelTime);
        }

        public void Draw(DrawBatch batch)
        {
            var rotation = -(float)Math.Atan2(Velocity.X, Velocity.Y);

            var rotMatrix = Matrix.CreateRotationZ(rotation)
                * Matrix.CreateTranslation(Position.X, Position.Y, 0);
            var rotShape = new Vector2[shape.Count];
            for (int i = 0; i < shape.Count; i++)
            {
                rotShape[i] = Vector2.Transform(shape[i], rotMatrix);
            }

            batch.FillTriangleList(brush, rotShape, tris);

            if (DrawGlow)
                glow.Draw(batch, rotShape, Color);
        }
    }
}