using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Svg.ExCSS;
using System;
using System.Diagnostics;

namespace NoisEvader
{
    public class ArenaCamera
    {
        public Matrix Transform { get; private set; }

        public float MainScale { get; set; }
        private Vector2 center;

        public float AniZoom { get; set; } = 1f;

        private Tweener slomoZoomTween = new Tweener(MathEx.EaseInOutSine);
        private Tweener slomoCameraPosTween = new Tweener(MathEx.EaseInOutSine);

        private const float NormalZoom = 1f;
        private const float SlomoZoom = 1.15f;
        private const float SlomoZoomAniDuration = 500f;

        public bool MouseFollowEnabled { get; set; }

        private float mouseFollowAmount = 0.05f;

        private SlomoState slomoState;

        public ArenaCamera()
            : this(Vector2.Zero) { }

        public ArenaCamera(Vector2 center)
        {
            this.center = center;
            SetMainScale();
            SetTransform(center);
        }

        public void SlomoZoomIn()
        {
            slomoState = SlomoState.Enabled;
            float duration = SlomoZoomAniDuration;
            if (slomoZoomTween.Enabled)
                duration -= slomoZoomTween.Elapsed;
            slomoZoomTween.Animate(AniZoom, SlomoZoom, duration);
            slomoCameraPosTween.Animate(0, 1, duration);
        }

        public Vector2 MouseToWorld(Vector2 mouse) =>
            MathEx.MouseToWorld(mouse, center, MainScale);

        public void SlomoZoomOut()
        {
            slomoState = SlomoState.Disabling;
            float duration = slomoZoomTween.Enabled ?
                slomoZoomTween.Elapsed : SlomoZoomAniDuration;
            slomoZoomTween.Animate(AniZoom, NormalZoom, duration);
            slomoCameraPosTween.Animate(1, 0, duration);
        }

        public void Update(GameTime gameTime)
        {
            Vector2 position = center;
            position = UpdateSlomo(gameTime, center, position);
            SetTransform(position);
        }

        private Vector2 UpdateSlomo(GameTime gameTime, Vector2 arenaCenter, Vector2 position)
        {
            // camera position
            if (slomoState != SlomoState.Disabled)
            {
                // mouse follow
                var mousePos = MouseToWorld(new Vector2(Mouse.GetState().X, Mouse.GetState().Y));
                var mouseOffset = (arenaCenter - mousePos) * mouseFollowAmount;
                position -= mouseOffset;

                // zoom
                slomoZoomTween.Update((float)gameTime.ElapsedGameTime.TotalMilliseconds);
                if (slomoZoomTween.Enabled)
                    AniZoom = slomoZoomTween.CurrentValue;

                slomoCameraPosTween.Update((float)gameTime.ElapsedGameTime.TotalMilliseconds);
                if (slomoCameraPosTween.Enabled)
                {
                    var factor = slomoCameraPosTween.CurrentValue;
                    var distance = position - arenaCenter;
                    position = arenaCenter + (distance * factor);
                }

                if (slomoState == SlomoState.Disabling && !slomoZoomTween.Enabled)
                    slomoState = SlomoState.Disabled;
            }

            return position;
        }

        public void SetTransform(Vector2 position)
        {
            Transform = Matrix.CreateTranslation(
                new Vector3(-position.X, -position.Y, 0)) * // camera position
                Matrix.CreateRotationZ(0) *
                Matrix.CreateScale(new Vector3(MainScale, MainScale, 1)) * // regular zoom
                Matrix.CreateScale(new Vector3(AniZoom, AniZoom, 1)) * // current slomo zoom, default 1
                Matrix.CreateTranslation(new Vector3(NoisEvader.ScreenCenter.X, NoisEvader.ScreenCenter.Y, 0));
        }

        public void SetMainScale()
        {
            switch (Config.GameSettings.ScaleMode)
            {
                case ScaleMode.Fit:
                    MainScale = Math.Min(NoisEvader.ScreenBounds.Width, NoisEvader.ScreenBounds.Height)
                        / NoisEvader.GameHeight;
                    break;
                case ScaleMode.Flash:
                    MainScale = NoisEvader.ScreenBounds.Width / NoisEvader.GameWidth;
                    break;
                default:
                    throw new NotImplementedException("Unknown scale mode");
            }
        }

        private enum SlomoState
        {
            Disabled,
            Enabled,
            Disabling,
        }
    }
}
