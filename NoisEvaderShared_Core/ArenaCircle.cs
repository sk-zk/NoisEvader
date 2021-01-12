using System;
using System.Collections.Generic;
using System.Text;
using LilyPath;
using LilyPath.Pens;
using Microsoft.Xna.Framework;

namespace NoisEvader
{
    public class ArenaCircle
    {
        public CircleHitbox Hitbox { get; private set; }

        public CircleSprite Circle { get; private set; }

        public bool GlowEnabled { get; set; }

        public CircleSprite SongPosCircle { get; private set; }
        public CircleSprite ScoreCircle { get; private set; }

        private GradientPen glowPen = new GradientPen(Color.Transparent,
            new Color(Color.White, 0.45f), 75);

        public ArenaCircle() : base()
        {
            Circle = new CircleSprite()
            {
                Subdivisions = 80,
                PenAlignment = PenAlignment.Outset,
                FillColor = Color.White,
                Radius = Arena.ArenaRadius,
                Center = Vector2.Zero,
                LineThickness = 2,
            };

            Hitbox = new CircleHitbox(Circle);

            SongPosCircle = new CircleSprite()
            {
                PenAlignment = PenAlignment.Outset,
            };

            ScoreCircle = new CircleSprite
            {
                LineThickness = 0f,
            };
        }

        public void SetScale(bool isLive)
        {
            var arenaRadius = Arena.ArenaRadius;
            if (isLive)
                arenaRadius *= 1.75f;
            Circle.Radius = arenaRadius;

            Circle.Center = isLive
                ? new Vector2(0, (NoisEvader.GameHeight * 0.6f) / 2f)
                : Vector2.Zero;

            Circle.LineThickness = 2;

            ScoreCircle.Center = Circle.Center;

            SongPosCircle.Center = Circle.Center;
            SongPosCircle.LineThickness = Circle.LineThickness;
        }

        public void SetColors(LevelColors colors, Color backgroundColor)
        {
            Circle.FillColor = backgroundColor;
            Circle.BorderColor = colors.Outline;

            ScoreCircle.Radius = 0f;
            ScoreCircle.FillColor = Util.GetActualScoreColor(colors.ScoreCircle);
            ScoreCircle.BorderColor = colors.Outline;

            SongPosCircle.Radius = 0f;
            SongPosCircle.FillColor = colors.Background;
            SongPosCircle.BorderColor = colors.Outline;
        }

        public void Draw(DrawBatch drawBatch)
        {
            Circle.Draw(drawBatch);
            SongPosCircle.Draw(drawBatch);
            ScoreCircle.Draw(drawBatch);
        }

        public void DrawGlow(DrawBatch drawBatch)
        {
            if (GlowEnabled)
                drawBatch.DrawCircle(glowPen, Circle.Center, Circle.Radius);
        }

    }
}
