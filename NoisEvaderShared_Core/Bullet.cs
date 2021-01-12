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
    public abstract class Bullet
    {
        /// <summary>
        /// The speed of a bullet in px/sec. at speed 1
        /// at a resolution of 896x504.
        /// </summary>
        public static double Speed1 = 30.0;

        protected Vector2 position;
        public Vector2 Position
        {
            get => position;
            set
            {
                if (float.IsNaN(value.X) || float.IsNaN(value.Y))
                    throw new ArithmeticException();

                position = value;
            }
        }

        protected Vector2 velocity;
        /// <summary>
        /// Velocity per millisecond.
        /// </summary>
        public Vector2 Velocity
        {
            get => velocity;
            set => velocity = value;
        }

        public float Age { get; set; }

        public float MinimumAge { get; set; } = 1000;

        protected Vector2 baseVelocity = Vector2.One;
        public Vector2 BaseVelocity
        {
            get => baseVelocity;
            set => baseVelocity = value;
        }

        /// <summary>
        /// Speed value according to the script.
        /// </summary>
        public float BaseSpeed
        {
            get => BaseVelocity.Length();
            set
            {
                BaseVelocity = new Vector2(
                    BaseVelocity.X * value / BaseSpeed,
                    BaseVelocity.Y * value / BaseSpeed);
            }
        }

        public float BaseRotation
        {
            get => (float)Math.Atan2(BaseVelocity.Y, BaseVelocity.X);
            set
            {
                BaseVelocity = new Vector2(
                    (float)(BaseSpeed * Math.Cos(value)),
                    (float)(BaseSpeed * Math.Sin(value)));
            }
        }

        public float BaseRotationDeg
        {
            get => MathHelper.ToDegrees(BaseRotation);
            set => BaseRotation = MathHelper.ToRadians(value);
        }

        protected Vector2 rotationOrigin;

        public IHitbox Hitbox { get; protected set; }

        protected HitType hit;
        public virtual HitType Hit
        {
            get => hit;
            set
            {
                hit = value;
                Color = HitColor;
            }
        }

        protected Color color;
        public virtual Color Color
        {
            get => color;
            set
            {
                color = value;
                brush = new SolidColorBrush(value);
            }
        }
        protected SolidColorBrush brush;

        public static readonly Color HitColor = Color.Red;

        protected bool isBackwards;

        public bool DrawGlow { get; set; }

        protected Bullet(Vector2 startingPosition, float speed, float rotation, Color color)
        {
            isBackwards = speed < 0;

            Position = startingPosition;
            BaseSpeed = Math.Abs(speed);
            BaseRotation = rotation;
            Color = color;

            DrawGlow = Config.GameSettings.DrawGlow;

            UpdateVelocity();
        }

        protected virtual void UpdateVelocity()
        {
            var pps = BaseSpeed * Speed1;
            var ppms = pps / 1000f; // pixels per millisecond

            velocity.X = (float)(Math.Sin(BaseRotation) * ppms * -1);
            velocity.Y = (float)(Math.Cos(BaseRotation) * ppms);
        }

        public abstract bool IsOutsideCircle(CircleHitbox circle);

        public abstract bool DespawnNow(CircleHitbox circle);

        public virtual void Update(LevelTime levelTime)
        {
            if (double.IsNaN(levelTime.TimeWarp) || double.IsNaN(levelTime.GameSpeed))
                throw new ArithmeticException();

            var vel = velocity;
            if (isBackwards)
                vel *= -1;

            position += vel * (float)levelTime.LevelElapsedMs;

            Age += (float)levelTime.ElapsedMs;
        }
    }

    public enum HitType
    {
        None,
        Normal,
        Heart,
    }
}