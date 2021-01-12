using LilyPath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Svg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NoisEvader
{
    public class Hug : Bullet, IDrawable
    {
        private const float HugRadius = 25f;
        private const float HitboxRadius = HugRadius;
        private const float DotRadius = 1.25f;
        private const float HuggedLineThickness = 2 * DotRadius;

        private const int HuggedSubdivs = 50;
        private const int UnhuggedSubdivs = 8;

        private CircleSprite actualHug;

        private CircleSprite huggedVis;
        private Tweener huggedVisAnim;
        private const float HuggedAnimDuration = 133;

        private Circle hitboxCircle;

        public bool IsHugged { get; set; }

        public override HitType Hit
        {
            get => hit;
            set
            {
                hit = value;
                Color = HitColor;
                actualHug.FillColor = HitColor;
            }
        }

        public override Color Color
        {
            get => color;
            set
            {
                color = value;
                brush = new SolidColorBrush(value);
                huggedPen = new Pen(value, HuggedLineThickness);
            }
        }
        private Pen huggedPen;

        public Hug(Vector2 startingPosition, float speed, float rotation, Color color)
            : base(startingPosition, speed, rotation, color)
        {
            actualHug = new CircleSprite(Vector2.Zero, HugRadius)
            {
                FillColor = Color,
                LineThickness = HuggedLineThickness,
                Center = startingPosition,
            };

            huggedVis = new CircleSprite(Vector2.Zero, actualHug.Radius)
            {
                LineThickness = 0,
                Subdivisions = HuggedSubdivs,
                BorderColor = Color,
                FillColor = Color.Transparent,
            };

            hitboxCircle = new Circle()
            {
                Radius = HitboxRadius,
                Center = startingPosition,
            };
            Hitbox = new CircleHitbox(hitboxCircle);
        }

        public void PlayerHugged()
        {
            if (IsHugged)
                return;

            IsHugged = true;
            huggedVisAnim = new Tweener((n) => n);
            huggedVisAnim.Animate(actualHug.Radius, actualHug.LineThickness, HuggedAnimDuration);
        }

        public override bool IsOutsideCircle(CircleHitbox circle) =>
            !circle.Intersects(Hitbox);

        public override bool DespawnNow(CircleHitbox circle) =>
            Age > MinimumAge && !circle.IsPointInside(actualHug.Center);

        public override void Update(LevelTime levelTime)
        {
            base.Update(levelTime);
            actualHug.Center = Position;
            hitboxCircle.Center = Position;

            if (IsHugged && huggedVisAnim?.Elapsed < HuggedAnimDuration)
            {
                huggedVisAnim.Update((float)levelTime.ElapsedMs);
                huggedVis.LineThickness = huggedVisAnim.CurrentValue;
                huggedVis.Center = actualHug.Center;
            }
        }

        public void Draw(DrawBatch batch)
        {
            if (IsHugged)
                DrawHugged(batch);
            else
                DrawUnhugged(batch);
        }

        private void DrawHugged(DrawBatch batch)
        {
            if (huggedVisAnim?.Elapsed < HuggedAnimDuration)
                huggedVis.Draw(batch);

            batch.DrawCircle(huggedPen, actualHug.Center, actualHug.Radius, HuggedSubdivs);
        }

        private void DrawUnhugged(DrawBatch batch)
        {
            const int amount = 26;
            const float distBetweenDots = (float)(2 * Math.PI / amount);
            for (int i = 0; i < amount; i++)
            {
                var angle = (i * distBetweenDots) - MathHelper.ToRadians(90);
                var x = (float)(actualHug.Center.X + (actualHug.Radius * Math.Cos(angle)));
                var y = (float)(actualHug.Center.Y + (actualHug.Radius * Math.Sin(angle)));
                var dotPos = new Vector2(x, y);
                batch.FillCircle(brush, dotPos, DotRadius, UnhuggedSubdivs);
            }
        }
    }
}
