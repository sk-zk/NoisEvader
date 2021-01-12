using Microsoft.Xna.Framework;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace NoisEvader.UI
{
    public class ScorePanel : VerticalStackPanel
    {
        private readonly SolidBrush background = new SolidBrush(Color.LightGray);
        private readonly SolidBrush hoverBackground = new SolidBrush(Color.LightSlateGray);

        public ScorePanel(Score score, bool levelHasHeart)
        {
            Build(score, levelHasHeart);
        }

        private void Build(Score score, bool levelHasHeart)
        {
            HorizontalAlignment = HorizontalAlignment.Stretch;
            Padding = new Thickness(5);
            Margin = new Thickness(0, 0, 5, 0);
            Background = background;
            MouseEntered += (_, __) => Background = hoverBackground;
            MouseLeft += (_, __) => Background = background;

            var scoreHeartPanel = new HorizontalStackPanel();
            Widgets.Add(scoreHeartPanel);

            if (levelHasHeart && score.HeartGotten)
            {
                var heart = new Image();
                UIUtil.SetHeart(heart, 18);
                heart.Renderable = LevelSelect.HeartIcon;
                scoreHeartPanel.Widgets.Add(heart);
            }

            var scoreTxt = new Label
            {
                Text = $"{score.Percent:0.00%} {score.Mod}"
            };

            scoreHeartPanel.Widgets.Add(scoreTxt);

            var date = new Label
            {
                Text = score.Time.ToLocalTime().ToString(CultureInfo.InstalledUICulture),
                TextColor = Color.DimGray,
                Font = Fonts.Orkney.GetFont(Fonts.PtToPx(13)),
            };
            Widgets.Add(date);
        }
    }
}
