using LilyPath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    class Waveform
    {
        private float[][] samples;
        public float[][] Samples
        {
            get => samples;
            set
            {
                samples = value;
                Render(NoisEvader.Gd);
            }
        }

        private float width;
        public float Width {  get => width; }

        private float height;
        public float Height { get => height; }

        public Vector2 Position { get; set; }

        public Color Color { get; set; } = Color.White;

        private RenderTarget2D target;

        public Waveform(float width, float height)
        {
            this.width = width;
            this.height = height;
            target = new RenderTarget2D(NoisEvader.Gd,
                (int)Math.Round(width, MidpointRounding.AwayFromZero),
                (int)Math.Round(height, MidpointRounding.AwayFromZero));
        }

        public void SetSize(float width, float height)
        {
            this.width = width;
            this.height = height;
            Render(NoisEvader.Gd);
        }

        private void Render(GraphicsDevice gd)
        {
            gd.SetRenderTarget(target);
            gd.Clear(Color.Transparent);
            var batch = new DrawBatch(NoisEvader.Gd);
            batch.Begin();
            DrawChannel(batch, 0);
            batch.End();
            gd.SetRenderTarget(null);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(target, Position, Color);
        }

        void DrawChannel(DrawBatch drawBatch, int channel)
        {
            var sampleWidth = Width / samples[channel].Length;
            var center = Height / 2;
            var pen = new Pen(Color.White, sampleWidth);

            for (int i = 0; i < samples[channel].Length; i++)
            {
                var yPos = samples[channel][i] * (Height / 2);
                var xPos = i * sampleWidth;
                drawBatch.DrawLine(pen,
                    new Vector2(xPos, center - yPos),
                    new Vector2(xPos, center + yPos));
            }
        }

    }
}
