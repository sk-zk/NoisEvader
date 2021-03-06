/* Generated by MyraPad at 10.06.2020 20:52:15 */
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
	partial class MessageBox: Window
	{
		private void BuildUI()
		{
			Message = new Label();
			Message.Text = "sample text";
			Message.Id = "Message";

			Ok = new TextButton();
			Ok.Text = "OK";
			Ok.MinWidth = 70;
			Ok.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Center;
			Ok.Id = "Ok";

			var verticalStackPanel1 = new VerticalStackPanel();
			verticalStackPanel1.Spacing = 10;
			verticalStackPanel1.Padding = new Thickness(5);
			verticalStackPanel1.Background = new SolidBrush("#ADADADFF");
			verticalStackPanel1.Widgets.Add(Message);
			verticalStackPanel1.Widgets.Add(Ok);

			
			Title = "NoisEvader";
			Left = 430;
			Top = 139;
			MinWidth = 200;
			Content = verticalStackPanel1;
		}

		
		public Label Message;
		public TextButton Ok;
	}
}