/* Generated by MyraPad at 17.09.2020 14:09:01 */
using Myra.Graphics2D;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;

#if !XENKO
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#else
using Xenko.Core.Mathematics;
#endif

namespace NoisEvader.UI
{
	partial class ModWindow: Window
	{
		private void BuildUI()
		{
			Showcase = new CheckBox();
			Showcase.Text = " Showcase";
			Showcase.TextColor = Color.White;
			Showcase.ImageWidth = 16;
			Showcase.ImageHeight = 16;
			Showcase.Id = "Showcase";

			Zen = new CheckBox();
			Zen.Text = " Zen";
			Zen.TextColor = Color.White;
			Zen.ImageWidth = 16;
			Zen.ImageHeight = 16;
			Zen.Id = "Zen";

			var horizontalSeparator1 = new HorizontalSeparator();
			horizontalSeparator1.Thickness = 2;
			horizontalSeparator1.Padding = new Thickness(0, 5);

			ChangeGameSpeed = new CheckBox();
			ChangeGameSpeed.ImageWidth = 16;
			ChangeGameSpeed.ImageHeight = 16;
			ChangeGameSpeed.Id = "ChangeGameSpeed";

			GameSpeed = new HorizontalSlider();
			GameSpeed.Minimum = 0.5f;
			GameSpeed.Maximum = 1.5f;
			GameSpeed.Value = 1;
			GameSpeed.Height = 16;
			GameSpeed.VerticalAlignment = Myra.Graphics2D.UI.VerticalAlignment.Center;
			GameSpeed.Id = "GameSpeed";

			GameSpeedLabel = new Label();
			GameSpeedLabel.Text = "1.0x";
			GameSpeedLabel.TextColor = Color.White;
			GameSpeedLabel.Id = "GameSpeedLabel";

			var horizontalStackPanel1 = new HorizontalStackPanel();
			horizontalStackPanel1.Spacing = 5;
			horizontalStackPanel1.Proportions.Add(new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Auto,
			});
			horizontalStackPanel1.Proportions.Add(new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Pixels,
				Value = 80,
			});
			horizontalStackPanel1.Proportions.Add(new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Auto,
			});
			horizontalStackPanel1.Widgets.Add(ChangeGameSpeed);
			horizontalStackPanel1.Widgets.Add(GameSpeed);
			horizontalStackPanel1.Widgets.Add(GameSpeedLabel);

			var horizontalSeparator2 = new HorizontalSeparator();
			horizontalSeparator2.Thickness = 2;
			horizontalSeparator2.Padding = new Thickness(0, 5);

			Flashlight = new CheckBox();
			Flashlight.Text = " Flashlight";
			Flashlight.TextColor = Color.White;
			Flashlight.ImageWidth = 16;
			Flashlight.ImageHeight = 16;
			Flashlight.Id = "Flashlight";

			Live = new CheckBox();
			Live.Text = " Live";
			Live.TextColor = Color.White;
			Live.ImageWidth = 16;
			Live.ImageHeight = 16;
			Live.Id = "Live";

			Ok = new TextButton();
			Ok.Text = "OK";
			Ok.TextColor = Color.White;
			Ok.Margin = new Thickness(0, 10, 0, 0);
			Ok.Padding = new Thickness(5);
			Ok.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Center;
			Ok.Id = "Ok";

			MainPanel = new VerticalStackPanel();
			MainPanel.Id = "MainPanel";
			MainPanel.Widgets.Add(Showcase);
			MainPanel.Widgets.Add(Zen);
			MainPanel.Widgets.Add(horizontalSeparator1);
			MainPanel.Widgets.Add(horizontalStackPanel1);
			MainPanel.Widgets.Add(horizontalSeparator2);
			MainPanel.Widgets.Add(Flashlight);
			MainPanel.Widgets.Add(Live);
			MainPanel.Widgets.Add(Ok);

			
			Title = "Mods";
			Left = 456;
			Top = 80;
			Padding = new Thickness(10);
			Content = MainPanel;
		}

		
		public CheckBox Showcase;
		public CheckBox Zen;
		public CheckBox ChangeGameSpeed;
		public HorizontalSlider GameSpeed;
		public Label GameSpeedLabel;
		public CheckBox Flashlight;
		public CheckBox Live;
		public TextButton Ok;
		public VerticalStackPanel MainPanel;
	}
}