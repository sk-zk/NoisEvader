using LilyPath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoisEvader
{
    public class Bubble : Bullet, IDrawable
    {
        private const float BubbleRadius = 25f;
        private const float HitboxRadius = 20f;

        protected CircleSprite bubbleCircle;
        protected BubbleGlow glow;

        private Circle hitboxCircle;

        public override HitType Hit
        {
            get => hit;
            set
            {
                hit = value;
                Color = HitColor;
                bubbleCircle.FillColor = HitColor;
            }
        }

        public Bubble(Vector2 startingPosition, float speed, float rotation, Color color)
            : base(startingPosition, speed, rotation, color)
        {
            bubbleCircle = new CircleSprite
            {
                Radius = BubbleRadius,
                FillColor = Color,
                LineThickness = 0f,
                Center = startingPosition,
                Subdivisions = 35,
            };

            glow = new BubbleGlow(BaseSpeed, BaseRotation, bubbleCircle);

            hitboxCircle = new Circle()
            {
                Radius = HitboxRadius,
                Center = startingPosition,
            };
            Hitbox = new CircleHitbox(hitboxCircle);
        }

        public override bool IsOutsideCircle(CircleHitbox circle) =>
            !circle.Intersects(Hitbox);

        public override bool DespawnNow(CircleHitbox circle) =>
            Age > MinimumAge && !circle.IsPointInside(bubbleCircle.Center);

        public override void Update(LevelTime levelTime)
        {
            base.Update(levelTime);
            bubbleCircle.Center = Position;
            hitboxCircle.Center = Position;
            glow.Update(levelTime);
        }

        public virtual void Draw(DrawBatch batch)
        {
            bubbleCircle.Draw(batch);

            if (DrawGlow)
                glow.Draw(batch, Color);
        }
    }
}
