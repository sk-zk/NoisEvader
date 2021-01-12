using LilyPath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoisEvader
{
    public class HeartSprite
    {
        public Vector2 Center { get; set; }

        public Color Color { get; set; } = Color.Red;

        public float Size { get; set; }

        public float BaseRotation { get; set; }

        private static Vector2 textureCenter;
        private static Texture2D texture;
        public static Texture2D Texture
        {
            get => texture;
            set
            {
                texture = value;
                textureCenter = new Vector2(texture.Width / 2, texture.Height / 2);
            }
        }

        public Matrix Transform { get; set; } = Matrix.Identity;

        private static List<Vector2> shape = new List<Vector2>();
        private static int[] tris;

        public HeartSprite(float size)
        {
            Size = size;
        }

        public static void SetShape(List<Vector2> verts, int[] tris)
        {
            HeartSprite.shape = verts;
            HeartSprite.tris = tris;
        }

        public void Draw(DrawBatch drawBatch)
        {
            var transform = Transform
                * Matrix.CreateScale(Size * 4) 
                * Matrix.CreateTranslation(new Vector3(Center, 0));
            var transformed = new List<Vector2>(shape);
            for (int i = 0; i < transformed.Count; i++)
                transformed[i] = Vector2.Transform(transformed[i], transform);

            drawBatch.FillTriangleList(new SolidColorBrush(Color), transformed, tris);
        }
    }
}
