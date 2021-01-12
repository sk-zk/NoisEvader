using CSCore.Tags.ID3.Frames;
using LilyPath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    class MenuScene
    {
        public static readonly Color BackgroundColor = new Color(0x12, 0x12, 0x1A);

        public ArenaCamera Camera { get; private set; }

        private ArenaCircle arena;

        private OuterRings rings;

        public MenuScene()
        {
            Camera = new ArenaCamera();

            arena = new ArenaCircle();
            arena.Circle.BorderColor = Color.CornflowerBlue;
            arena.Circle.FillColor = Color.White;
            arena.Circle.LineThickness = 3;
            arena.GlowEnabled = true;
            arena.Hitbox.Inverted = true;

            rings = new OuterRings(new Color(0x37, 0x71, 0xc8), arena.Circle);
        }

        public void OnWindowResized()
        {
            Camera.SetMainScale();
            Camera.SetTransform(arena.Circle.Center);
        }

        public void Update(GameTime gt)
        {
            var ringTime = new LevelTime()
            {
                GameTime = gt,
                GameSpeed = 0.6f,
                SongPosition = 1,
                TimeWarp = 1
            };
            rings.Update(ringTime);
        }

        public void Draw(SpriteBatch spriteBatch, DrawBatch drawBatch)
        {
            rings.Draw(drawBatch);
            arena.DrawGlow(drawBatch);
            arena.Draw(drawBatch);
        }
    }
}
