using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader.UI
{
    class LevelBoxFolder : LevelBoxButton
    {
        public LevelBoxFolder(string displayName)
        {
            var label = new Label();
            Widgets.Add(label);
            label.Text = $"[ {displayName} ]";
        }
    }
}
