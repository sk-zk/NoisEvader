using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    public class SimpleText
    {
        private string text;
        public string Text
        {
            get => text;
            set
            {
                text = value;
                CalculateSize();
            }
        }

        public SpriteFont Font
        {
            get => font;
            set
            {
                font = value;
                CalculateSize();
            }
        }

        public Vector2 Position { get; set; }

        public Color Color { get; set; }

        /// <summary>
        /// Sets the horizontal location the Position node refers to.
        /// </summary>
        public XOrigin XOrigin { get; set; }

        /// <summary>
        /// Sets the vertical location the Position node refers to.
        /// </summary>
        public YOrigin YOrigin { get; set; }

        public bool Visible { get; set; } = true;

        private Vector2 size;
        private SpriteFont font;

        private void CalculateSize()
        {
            if (font is null || string.IsNullOrEmpty(text))
                size = Vector2.Zero;
            else
                size = font.MeasureString(text);
        }

        public void Draw(SpriteBatch batch)
        {
            if (!Visible)
                return;

            var realPos = Position;
            switch (XOrigin)
            {
                default: break;
                case XOrigin.Center: realPos.X -= size.X / 2; break;
                case XOrigin.Right: realPos.X -= size.X; break;
            }
            switch (YOrigin)
            {
                default: break;
                case YOrigin.Center: realPos.Y -= size.Y / 2; break;
                case YOrigin.Bottom: realPos.Y -= size.Y; break;
            }
            batch.DrawString(Font, Text, realPos, Color);
        }
    }

    public enum XOrigin
    {
        Left,
        Center,
        Right,
    }

    public enum YOrigin
    {
        Top,
        Center,
        Bottom,
    }
}
