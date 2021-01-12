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
    public class Homing : TrailBullet, IDrawable
    {
        public const float HomingSize = 20f;
        public const float DefaultLifespan = 3000;

        public bool IsHoming => lifespan > 0;

        private float startHoming = 500;
        private float lifespan = DefaultLifespan;

        public Vector2 Tip =>
            // surround the bullet with a circle & get the corner
            Position + GetCorner(1);

        public Vector2 Back =>
            Position + GetCorner(-1);

        public override HitType Hit
        {
            get => base.Hit;
            set
            {
                base.Hit = value;
                if (value != HitType.None)
                    particleParameters.Color = HitColor;

            }
        }

        protected HomingGlow glow;

        public Homing(Vector2 startingPosition, float speed, float rotation, float lifespan, Color color) 
            : base(startingPosition, speed, rotation, color)
        {
            rotationOrigin = new Vector2(HomingSize / 2, HomingSize / 2);
            this.lifespan = lifespan;

            glow = new HomingGlow(BaseSpeed);

            Hitbox = new PointHitbox()
            {
                Point = startingPosition,
            };
        }

        public override bool IsOutsideCircle(CircleHitbox circle) =>
            !circle.IsPointInside(Tip);

        public override bool DespawnNow(CircleHitbox circle) =>
            Age > MinimumAge && !circle.IsPointInside(Tip);

        public void Update(LevelTime levelTime, Vector2 playerCenter)
        {
            Update(levelTime);

            var elapsed = (float)levelTime.LevelElapsedMs;

            if (IsHoming)
            {
                if (startHoming > 0)
                {
                    startHoming -= elapsed;
                }
                else
                {
                    Home(elapsed, playerCenter);
                    UpdateVelocity();
                    lifespan -= elapsed;
                }
            }

            (Hitbox as PointHitbox).Point = Tip;

            spawnParticles = IsHoming;
            particleParameters.SystemCenter = Back;
            UpdateParticles(levelTime);
            glow.Update(levelTime);
        }

        protected override void UpdateVelocity()
        {
            base.UpdateVelocity();
            if (isBackwards)
                velocity *= -1;
        }

        private void Home(float elapsed, Vector2 player)
        {
            const float turningSpeed = 1f / 160f;

            var desired = Vector2.Normalize(Position - player);
            var desiredRot = (float)Math.Atan2(desired.Y, desired.X) + MathHelper.ToRadians(90);
            desiredRot = MathHelper.WrapAngle(desiredRot);
            var deltaRot = BaseRotation - desiredRot;
            var turnAmt = turningSpeed * elapsed;
            if (Math.Abs(deltaRot) < MathHelper.ToRadians(180))
            {
                BaseRotation -= MathHelper.WrapAngle(deltaRot * turnAmt);
            }
            else
            {
                var plswork = (BaseRotation < desiredRot) ?
                    MathHelper.ToRadians(360) : MathHelper.ToRadians(-360);
                BaseRotation -= MathHelper.WrapAngle((deltaRot + plswork) * turnAmt);
            }
        }

        public virtual void Draw(DrawBatch batch)
        {
            particles.Draw(batch as DrawBatch);

            // homings are diamonds as far as this code is concerned
            // so to draw them with FillRectangle we need to rotate it
            var rot = BaseRotation + MathHelper.ToRadians(45);

            batch.FillRectangle(brush, Position - rotationOrigin,
                HomingSize, HomingSize, rot);

            glow.Draw(batch, this, Color);
        }

        public Vector2 GetCorner(float whichOne)
        {
            var rotation = MathHelper.ToRadians(BaseRotationDeg + (whichOne * 90));
            var radius = HomingSize * MathEx.HalfSqrt2;
            return MathEx.PointOnCircumference(Vector2.Zero, radius, rotation);
        }
    }
}
