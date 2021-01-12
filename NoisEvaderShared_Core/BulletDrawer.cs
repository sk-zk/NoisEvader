using LilyPath;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoisEvader
{
    public class BulletDrawer
    {
        private List<Bullet> bullets;

        public BulletDrawer(List<Bullet> bullets)
        {
            this.bullets = bullets;
        }

        public void Draw(SpriteBatch spriteBatch, DrawBatch drawBatch)
        {
            Draw(spriteBatch, drawBatch, bullets.Where(x => x is Bubble));
            Draw(spriteBatch, drawBatch, bullets.Where(x => !(x is Bubble)));
        }

        private void Draw(SpriteBatch spriteBatch, DrawBatch drawBatch, IEnumerable<Bullet> bullets)
        {
            foreach (Bullet bullet in bullets)
            {
                if (bullet is IDrawable d)
                    d.Draw(drawBatch);
                else
                    throw new NotImplementedException("Bullet of type " +
                        bullet.GetType().FullName + " has no IDrawable*?");

                if (false)
                    bullet.Hitbox.Draw(drawBatch);
            }
        }
    }
}
