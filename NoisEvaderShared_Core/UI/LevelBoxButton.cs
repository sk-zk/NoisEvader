using Microsoft.Xna.Framework;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader.UI
{
    abstract class LevelBoxButton : Grid
    {
        public static readonly SolidBrush UnselectedBackground
            = new SolidBrush(new Color(0xac, 0xc3, 0xff, 0xff));
        public static readonly SolidBrush SelectedBackground
            = new SolidBrush(new Color(0x55, 0x99, 0xff, 0xff));
        public static readonly SolidBrush HoverBackground
            = new SolidBrush(Color.Lerp(UnselectedBackground.Color, SelectedBackground.Color, 0.5f));

        public LevelBoxButton() : base()
        {
            DefaultColumnProportion = new Proportion(ProportionType.Pixels, 540);
            DefaultRowProportion = new Proportion(ProportionType.Auto);
            Padding = new Thickness(5);
            Margin = new Thickness(0, 0, 10, 0);
            Width = 580;
            Background = UnselectedBackground;
        }

    }
}
