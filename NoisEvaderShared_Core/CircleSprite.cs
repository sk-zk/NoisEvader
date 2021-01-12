using LilyPath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Svg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoisEvader
{
    public class CircleSprite : Circle
    {
        public float InnerRadius => Radius - LineThickness;

        public Matrix Transform { get; set; } = Matrix.Identity;

        public int Subdivisions { get; set; } = 64;

        private float lineThickness;
        public float LineThickness
        {
            get => lineThickness;
            set
            {
                lineThickness = value;
                borderPen = new Pen(borderColor, value, PenAlignment);
            }
        }

        private Color fillColor;
        public Color FillColor
        {
            get => fillColor;
            set
            {
                fillColor = value;
                fillBrush = new SolidColorBrush(value);
            }
        }

        private Color borderColor;
        public Color BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                borderPen = new Pen(value, LineThickness, PenAlignment);
            }
        }

        protected SolidColorBrush fillBrush;
        protected Pen borderPen;

        private PenAlignment penAlignment = PenAlignment.Inset;
        public PenAlignment PenAlignment
        {
            get => penAlignment;
            set
            {
                penAlignment = value;
                if (borderPen is { } p)
                    p.Alignment = value;
            }
        }

        /// <summary>
        /// If true, the border is drawn with a simplified draw method,
        /// which is faster but doesn't support transforms and most pen features (such as gradients).
        /// </summary>
        public bool UseSimpleDraw { get; set; } = true;

        public CircleSprite()
        {
            FillColor = Color.White;
            BorderColor = Color.Black;
        }

        public CircleSprite(Vector2 position, float radius)
        {
            Position = position;
            Radius = radius;
        }

        private bool HasBorder() =>
            borderPen.Width > 0.001 && Radius > 0;

        public virtual void Draw(DrawBatch drawBatch)
        {
            Draw(drawBatch, fillBrush, borderPen);
        }

        public virtual void Draw(DrawBatch drawBatch, SolidColorBrush fillBrush)
        {
            Draw(drawBatch, fillBrush, borderPen);
        }

        public virtual void Draw(DrawBatch drawBatch, SolidColorBrush fillBrush, Pen borderPen)
        {
            if (Radius < 0)
                return;

            drawBatch.FillCircle(fillBrush, Center, Radius, Subdivisions, Transform);
            if (HasBorder())
                DrawBorder(drawBatch, borderPen);
        }

        public virtual void DrawFilledOnly(DrawBatch drawBatch)
        {
            if (Radius < 0)
                return;

            drawBatch.FillCircle(fillBrush, Center, Radius, Subdivisions, Transform);
        }

        public virtual void DrawBorderOnly(DrawBatch drawBatch)
        {
            if (!HasBorder())
                return;

            DrawBorder(drawBatch, borderPen);
        }

        private void DrawBorder(DrawBatch drawBatch, Pen borderPen)
        {
            if (UseSimpleDraw)
                drawBatch.DrawCircleSimple(borderPen, Center, Radius, Subdivisions);
            else
                drawBatch.DrawCircle(borderPen, Center, Radius, Subdivisions, Transform);
        }

    }
}
