using LilyPath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoisEvader
{
    public abstract class Player : CircleSprite
    {
        public const float PlayerRadius = 10f;
        private const float HitboxRadius = 5f;

        public override Vector2 Position 
        { 
            get => base.Position;
            set
            {
                base.Position = value;
                SetCenters(Center);
            }
        }

        public override Vector2 Center
        { 
            get => base.Center;
            set
            {
                base.Center = value;
                SetCenters(value);
            }
        }

        protected Vector2 prevCenter;
        protected Vector2 velocityPerMs;

        public CircleHitbox Hitbox { get; set; }

        private CircleSprite hitboxVis { get; set; }

        public virtual InvincibilityType Invincibility { get; set; }
            = InvincibilityType.None;

        public float InvincibilityLength
        {
            get
            {
                switch (Invincibility)
                {
                    case InvincibilityType.Normal:
                        return 6000;
                    case InvincibilityType.Heart:
                        return 3000;
                    default:
                    case InvincibilityType.None:
                        return 0;
                }
            }
        }

        public virtual bool HasHeart { get; set; }

        private Color hitColor = Color.Red;
        private Color hitboxColor = Color.Black;

        public new Color BorderColor
        {
            get => base.BorderColor;
            set
            {
                base.BorderColor = value;
                oobShadow.BorderColor = value;
            }
        }

        protected HeartSprite heart;
        protected CircleSprite hitVis;

        protected CircleSprite oobShadow;

        protected double msSinceLastHit;

        protected InputHelper inputHelper = new InputHelper();

        public Player()
        {
            Radius = PlayerRadius;
            Center = Vector2.Zero;
            LineThickness = 2f;
            Subdivisions = 50;
            FillColor = Color.White;
            UseSimpleDraw = false;
            PenAlignment = PenAlignment.Inset;

            hitboxVis = new CircleSprite(Vector2.Zero, HitboxRadius)
            {
                LineThickness = 0,
                FillColor = hitboxColor,
                Subdivisions = 40,
                UseSimpleDraw = false
            };
            Hitbox = new CircleHitbox(hitboxVis);

            hitVis = new CircleSprite(Vector2.Zero, Radius)
            {
                LineThickness = 0,
                Radius = 0,
                Subdivisions = Subdivisions,
                FillColor = Color.White,
                BorderColor = hitColor,
                UseSimpleDraw = false
            };

            oobShadow = new CircleSprite(Vector2.Zero, Radius)
            {
                LineThickness = LineThickness,
                Subdivisions = Subdivisions,
                UseSimpleDraw = false,
                PenAlignment = PenAlignment
            };

            heart = new HeartSprite(InnerRadius * 2);
        }

        public virtual void Hit(InvincibilityType type = InvincibilityType.Normal)
        {
            if (Invincibility != InvincibilityType.None)
                return;

            Invincibility = type;
            msSinceLastHit = 0;
        }

        public void HeartLost()
        {
            HasHeart = false;
            Hit(InvincibilityType.Heart);
        }

        public virtual void CancelInvincibility()
        {
            Invincibility = InvincibilityType.None;
            msSinceLastHit = 0;
        }

        public virtual void Update(LevelTime levelTime, CircleSprite arenaCircle, ArenaCamera camera,
            bool confineCursorToScreen)
        {
            inputHelper.Update();
            prevCenter = Center;
            UpdatePosition(levelTime, arenaCircle, camera, confineCursorToScreen);
            velocityPerMs = (prevCenter - Center)
                / (float)levelTime.ElapsedMs;

            UpdateInvincibility(levelTime);
        }

        protected abstract void UpdatePosition(LevelTime levelTime, CircleSprite arenaCircle,
            ArenaCamera camera, bool confineCursorToScreen);

        protected virtual void UpdateInvincibility(LevelTime levelTime)
        {
            if (Invincibility == InvincibilityType.None ||
                Invincibility == InvincibilityType.Eternal)
                return;

            msSinceLastHit += levelTime.ElapsedMs;
            UpdateHitVis();

            if (msSinceLastHit >= InvincibilityLength)
                CancelInvincibility();
        }

        protected void UpdateHitVis()
        {
            var progress = (float)((InvincibilityLength - msSinceLastHit) / InvincibilityLength);
            var hitVisSize = InnerRadius * progress;
            hitVis.LineThickness = hitVisSize;
            hitVis.Radius = Radius - (hitVisSize);
        }

        protected void SetCenters(Vector2 newCenter)
        {
            if (hitboxVis != null) hitboxVis.Center = newCenter;
            if (heart != null) heart.Center = newCenter;
            if (hitVis != null) hitVis.Center = newCenter;
        }

        public override void Draw(DrawBatch drawBatch)
        {
            if (Config.GameSettings.StretchyPlayer)
                SetTransform();

            var hitAnimActive = Invincibility != InvincibilityType.None
                && Invincibility != InvincibilityType.Eternal;

            if (hitAnimActive)
            {
                hitVis.Draw(drawBatch);
                base.DrawBorderOnly(drawBatch);
            }
            else
            {
                base.Draw(drawBatch);
            }

            if (HasHeart)
                heart.Draw(drawBatch);

            if (Config.GameSettings.DrawPlayerHitbox)
                hitboxVis.Draw(drawBatch);
        }

        private void SetTransform()
        {
            const float epsilon = 0.1f;
            const float squishinessFactor = 0.15f;

            if (velocityPerMs.X > epsilon || velocityPerMs.Y > epsilon)
            {
                // TODO Fix heart rotation

                var vel = Math.Abs(velocityPerMs.Length());
                double scaleX = 1 - (vel * squishinessFactor);
                double scaleY = 1;
                double rot = Math.Atan2(velocityPerMs.Y, velocityPerMs.X) + (90 * MathEx.DegToRad);
                Transform = Matrix.CreateScale((float)scaleX, (float)scaleY, 1) 
                    * Matrix.CreateRotationZ((float)rot);
            }
            else
            {
                Transform = Matrix.CreateScale(1, 1, 1);
            }
            hitVis.Transform = Transform;
            hitboxVis.Transform = Transform;
            heart.Transform = Transform;
        }

        public void DrawShadow(DrawBatch drawBatch)
        {
            oobShadow.DrawBorderOnly(drawBatch);
        }
    }
}
