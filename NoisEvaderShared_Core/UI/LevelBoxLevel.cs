using Microsoft.Xna.Framework;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader.UI
{
    class LevelBoxLevel : LevelBoxButton
    {
        public LevelBoxLevel() : base() { }

        public LevelBoxLevel(CachedLevelData level) : this()
        {
            var gridRow = 0;

            var heartAndTitle = new HorizontalStackPanel();
            heartAndTitle.GridRow = gridRow++;
            Widgets.Add(heartAndTitle);

            if (level.Info.HasHeart)
            {
                var heart = new Image();
                UIUtil.SetHeart(heart, 18);
                heart.Renderable = level.HeartGotten
                    ? LevelSelect.HeartIcon
                    : LevelSelect.HeartOutlineIcon;
                heartAndTitle.Widgets.Add(heart);
            }

            var title = new Label
            {
                AutoEllipsisMethod = AutoEllipsisMethod.Character,
                Margin = new Thickness(0, 0, 10, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                Font = Fonts.OrkneyWithFallback.GetFont(24),
                Text = level.Info.Title,
            };
            heartAndTitle.Widgets.Add(title);

            var artist = new Label
            {
                GridRow = gridRow++,
                Width = Width - 5,
                AutoEllipsisMethod = AutoEllipsisMethod.Character,
                Font = Fonts.OrkneyWithFallback.GetFont(20),
                Text = level.Info.Artist,
            };
            Widgets.Add(artist);

            var diffAndSubtitle = new HorizontalStackPanel
            {
                GridRow = gridRow++,
                Spacing = 10
            };
            Widgets.Add(diffAndSubtitle);

            var diff = new Label
            {
                Text = UIUtil.GetDifficultyString(level.Info.Difficulty),
                Font = Fonts.DejaVuSans.GetFont(20),
                VerticalAlignment = VerticalAlignment.Center
            };
            diffAndSubtitle.Widgets.Add(diff);

            var subtitle = new Label
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            diffAndSubtitle.Widgets.Add(subtitle);
        }

    }
}
