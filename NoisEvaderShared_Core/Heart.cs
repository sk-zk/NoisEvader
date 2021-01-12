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
    public class Heart : TrailBullet, IDrawable
    {
        public const float HeartSize = 25f;

        private Circle hitboxCircle;

        private HeartSprite sprite;

        private Vector2 Tip => new Vector2(
            position.X - ((float)Math.Sin(BaseRotation) * (HeartSize * 0.5f)),
            position.Y - ((float)Math.Cos(BaseRotation) * (HeartSize * -0.5f))
        );

        private Vector2 Back => new Vector2(
            position.X + ((float)Math.Sin(BaseRotation) * (HeartSize * 0.5f)),
            position.Y + ((float)Math.Cos(BaseRotation) * (HeartSize * -0.5f))
        );

        public override HitType Hit
        {
            get => hit;
            set
            {
                hit = value;
                Color = HitColor;
            }
        }

        public Heart() : this(Vector2.Zero, 0, 0) { }

        public Heart(Vector2 startingPosition, float speed, float rotation)
            : base(startingPosition, speed, rotation, Color.White)
        {
            sprite = new HeartSprite(HeartSize)
            {
                Center = startingPosition,
            };

            hitboxCircle = new Circle()
            {
                Radius = HeartSize / 2,
                Center = startingPosition,
            };
            Hitbox = new CircleHitbox(hitboxCircle);
            Color = Color.Red;

            spawnParticles = true;
            particleParameters.Color = Color;

            UpdateVelocity();
        }

        public override bool IsOutsideCircle(CircleHitbox circle) =>
            !circle.Intersects(Hitbox);

        public override bool DespawnNow(CircleHitbox circle) =>
            Hit == HitType.Normal || (Age > MinimumAge && !circle.IsPointInside(hitboxCircle.Center));

        public override void Update(LevelTime levelTime)
        {
            levelTime.TimeWarp = 1; // hearts are unaffected by tw
            base.Update(levelTime);
            UpdateChildObjectPositions();
            UpdateParticles(levelTime);
        }

        private void UpdateChildObjectPositions()
        {
            hitboxCircle.Center = Position;
            sprite.Center = hitboxCircle.Center;
            sprite.BaseRotation = BaseRotation;
            particleParameters.SystemCenter = Back;
        }

        public void Draw(DrawBatch drawBatch)
        {
            particles.Draw(drawBatch);
            sprite.Draw(drawBatch);
        }

    }
}
