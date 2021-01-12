using LilyPath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoisEvader
{
    public class HumanPlayer : Player
    {
        private const float KeyboardNormalSpeed = 0.5f;
        private const float KeyboardSlowSpeed = 0.25f;

        public uint TotalHits { get; private set; }

        public HumanPlayer() : base()
        {
        }

        protected override void UpdatePosition(LevelTime levelTime, CircleSprite arenaCircle, ArenaCamera camera,
            bool confineCursorToScreen)
        {
            // TODO: Allow rebinding
            if (HasKeyboardMovement())
                Center = DoKeyboardMovement(levelTime, inputHelper.KeyboardState);
            else if (HasMouseMovement())
                Center = DoMouseMovement(camera, confineCursorToScreen);

            oobShadow.Center = camera.MouseToWorld(new Vector2(inputHelper.MouseState.X, inputHelper.MouseState.Y));

            KeepPlayerInbounds(arenaCircle, Center);
            SetCenters(Center);
        }

        private Vector2 DoMouseMovement(ArenaCamera camera, bool confineCursorToScreen)
        {
            var newPos = new Vector2(inputHelper.MouseState.X, inputHelper.MouseState.Y);

            if (confineCursorToScreen)
            {
                newPos.X = MathHelper.Clamp(newPos.X, 0, NoisEvader.ScreenBounds.Width);
                newPos.Y = MathHelper.Clamp(newPos.Y, 0, NoisEvader.ScreenBounds.Height);
            }

            return camera.MouseToWorld(newPos);
        }

        private Vector2 DoKeyboardMovement(LevelTime levelTime, KeyboardState state)
        {
            var movement = Vector2.Zero;
            if (state.IsKeyDown(Keys.Left))
                movement.X--;
            if (state.IsKeyDown(Keys.Right))
                movement.X++;
            if (state.IsKeyDown(Keys.Up))
                movement.Y--;
            if (state.IsKeyDown(Keys.Down))
                movement.Y++;

            if (movement == Vector2.Zero)
                return Center;

            movement.Normalize();
            movement *= state.IsKeyDown(Keys.LeftControl)
                ? KeyboardSlowSpeed : KeyboardNormalSpeed;
            movement *= (float)levelTime.ElapsedMs;
            return Center + movement;
        }

        private bool HasKeyboardMovement()
        {
            Keys[] movementKeys =
            {
                Keys.Left,
                Keys.Right,
                Keys.Up,
                Keys.Down,
            };
            return inputHelper.KeyboardState.GetPressedKeys()
                .Any(x => movementKeys.Contains(x));
        }

        private bool HasMouseMovement() =>
            inputHelper.MouseVelocity != Vector2.Zero;

        private void KeepPlayerInbounds(CircleSprite arenaCircle, Vector2 newPos)
        {
            float distance = Vector2.Distance(arenaCircle.Center, newPos);

            // is it outside the circle?
            if (distance > Math.Abs(Radius - arenaCircle.Radius))
            {
                var collision = newPos - arenaCircle.Center;
                var radians = (float)Math.Atan2(collision.Y, collision.X);
                Center = new Vector2(
                    arenaCircle.Center.X + (arenaCircle.Radius - Radius) * (float)Math.Cos(radians),
                    arenaCircle.Center.Y + (arenaCircle.Radius - Radius) * (float)Math.Sin(radians)
                    );
            }
        }

        public override void Hit(InvincibilityType type = InvincibilityType.Normal)
        {
            if (Invincibility != InvincibilityType.None)
                return;

            base.Hit(type);

            TotalHits++;
        }
    }

    public enum InvincibilityType
    {
        None,
        Normal,
        Heart,
        Eternal,
    }
}
