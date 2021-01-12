/* Generated by MyraPad at 27.10.2020 12:11:09 */
using Myra.Graphics2D;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;

#if !STRIDE
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#else
using Stride.Core.Mathematics;
#endif

namespace NoisEvader.UI
{
	partial class LevelSelect: Grid
	{
		private void BuildUI()
		{
			HideFolders = new TextButton();
			HideFolders.Text = "Hide folders";
			HideFolders.TextColor = Color.White;
			HideFolders.Toggleable = true;
			HideFolders.Padding = new Thickness(5);
			HideFolders.Id = "HideFolders";

			var label1 = new Label();
			label1.Text = "Sort by:";
			label1.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;
			label1.GridColumn = 1;

			SortMode = new ComboBox();
			SortMode.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;
			SortMode.GridColumn = 2;
			SortMode.Id = "SortMode";

			var grid1 = new Grid();
			grid1.ColumnSpacing = 8;
			grid1.ColumnsProportions.Add(new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Fill,
			});
			grid1.ColumnsProportions.Add(new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Auto,
			});
			grid1.ColumnsProportions.Add(new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Auto,
			});
			grid1.Widgets.Add(HideFolders);
			grid1.Widgets.Add(label1);
			grid1.Widgets.Add(SortMode);

			LevelScrollViewer = new ScrollViewer();
			LevelScrollViewer.Content = null;
			LevelScrollViewer.ShowHorizontalScrollBar = false;
			LevelScrollViewer.Id = "LevelScrollViewer";

			Back = new TextButton();
			Back.Text = "< Back to title";
			Back.TextColor = Color.White;
			Back.Padding = new Thickness(5);
			Back.Id = "Back";

			var verticalStackPanel1 = new VerticalStackPanel();
			verticalStackPanel1.Spacing = 8;
			verticalStackPanel1.Proportions.Add(new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Auto,
			});
			verticalStackPanel1.Proportions.Add(new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Fill,
			});
			verticalStackPanel1.Proportions.Add(new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Auto,
			});
			verticalStackPanel1.Widgets.Add(grid1);
			verticalStackPanel1.Widgets.Add(LevelScrollViewer);
			verticalStackPanel1.Widgets.Add(Back);

			HeartImg = new Image();
			HeartImg.VerticalAlignment = Myra.Graphics2D.UI.VerticalAlignment.Center;
			HeartImg.Id = "HeartImg";

			Title = new Label();
			Title.Text = "Title";
			Title.AutoEllipsisMethod = Myra.Graphics2D.UI.AutoEllipsisMethod.Character;
			Title.Id = "Title";

			var horizontalStackPanel1 = new HorizontalStackPanel();
			horizontalStackPanel1.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Left;
			horizontalStackPanel1.Widgets.Add(HeartImg);
			horizontalStackPanel1.Widgets.Add(Title);

			Artist = new Label();
			Artist.Text = "Artist";
			Artist.AutoEllipsisMethod = Myra.Graphics2D.UI.AutoEllipsisMethod.Character;
			Artist.Id = "Artist";

			Difficulty = new Label();
			Difficulty.Text = "OOOOO";
			Difficulty.Id = "Difficulty";

			Subtitle = new Label();
			Subtitle.Text = "Subtitle";
			Subtitle.Id = "Subtitle";

			var horizontalStackPanel2 = new HorizontalStackPanel();
			horizontalStackPanel2.Spacing = 5;
			horizontalStackPanel2.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Left;
			horizontalStackPanel2.Widgets.Add(Difficulty);
			horizontalStackPanel2.Widgets.Add(Subtitle);

			Length = new Label();
			Length.Text = "2:34";
			Length.Id = "Length";

			Creator = new Label();
			Creator.Text = "NotBean";
			Creator.Id = "Creator";

			var horizontalStackPanel3 = new HorizontalStackPanel();
			horizontalStackPanel3.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Left;
			horizontalStackPanel3.Widgets.Add(Length);
			horizontalStackPanel3.Widgets.Add(Creator);

			Play = new TextButton();
			Play.Text = "Play";
			Play.TextColor = Color.White;
			Play.Padding = new Thickness(5);
			Play.Id = "Play";

			Mods = new TextButton();
			Mods.Text = "Mods";
			Mods.TextColor = Color.White;
			Mods.Padding = new Thickness(5);
			Mods.Id = "Mods";

			LevelSettings = new TextButton();
			LevelSettings.Text = "Level settings";
			LevelSettings.TextColor = Color.White;
			LevelSettings.Padding = new Thickness(5);
			LevelSettings.Id = "LevelSettings";

			var horizontalStackPanel4 = new HorizontalStackPanel();
			horizontalStackPanel4.Spacing = 5;
			horizontalStackPanel4.Padding = new Thickness(0, 2, 0, 0);
			horizontalStackPanel4.Widgets.Add(Mods);
			horizontalStackPanel4.Widgets.Add(LevelSettings);

			var verticalStackPanel2 = new VerticalStackPanel();
			verticalStackPanel2.Spacing = 4;
			verticalStackPanel2.Widgets.Add(horizontalStackPanel1);
			verticalStackPanel2.Widgets.Add(Artist);
			verticalStackPanel2.Widgets.Add(horizontalStackPanel2);
			verticalStackPanel2.Widgets.Add(horizontalStackPanel3);
			verticalStackPanel2.Widgets.Add(Play);
			verticalStackPanel2.Widgets.Add(horizontalStackPanel4);

			ScoresHeader = new Label("Header");
			ScoresHeader.Text = "Scores";
			ScoresHeader.Id = "ScoresHeader";

			ScoresContainer = new Grid();
			ScoresContainer.RowSpacing = 5;
			ScoresContainer.DefaultColumnProportion = new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Pixels,
				Value = 300,
			};
			ScoresContainer.Id = "ScoresContainer";

			var scrollViewer1 = new ScrollViewer();
			scrollViewer1.ShowHorizontalScrollBar = false;
			scrollViewer1.Content = ScoresContainer;

			var verticalStackPanel3 = new VerticalStackPanel();
			verticalStackPanel3.Widgets.Add(ScoresHeader);
			verticalStackPanel3.Widgets.Add(scrollViewer1);

			DetailsHeader = new Label("Header");
			DetailsHeader.Text = "Details";
			DetailsHeader.Id = "DetailsHeader";

			Playcount = new Label();
			Playcount.Id = "Playcount";

			var verticalStackPanel4 = new VerticalStackPanel();
			verticalStackPanel4.Widgets.Add(DetailsHeader);
			verticalStackPanel4.Widgets.Add(Playcount);

			var horizontalStackPanel5 = new HorizontalStackPanel();
			horizontalStackPanel5.Spacing = 8;
			horizontalStackPanel5.Proportions.Add(new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Pixels,
				Value = 300,
			});
			horizontalStackPanel5.Proportions.Add(new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Fill,
			});
			horizontalStackPanel5.Margin = new Thickness(0, 40, 0, 0);
			horizontalStackPanel5.Widgets.Add(verticalStackPanel3);
			horizontalStackPanel5.Widgets.Add(verticalStackPanel4);

			LevelInfo = new VerticalStackPanel();
			LevelInfo.Spacing = 8;
			LevelInfo.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Left;
			LevelInfo.VerticalAlignment = Myra.Graphics2D.UI.VerticalAlignment.Top;
			LevelInfo.GridColumn = 1;
			LevelInfo.Id = "LevelInfo";
			LevelInfo.Widgets.Add(verticalStackPanel2);
			LevelInfo.Widgets.Add(horizontalStackPanel5);

			
			ColumnSpacing = 10;
			DefaultRowProportion = new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Fill,
			};
			ColumnsProportions.Add(new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Pixels,
				Value = 530,
			});
			ColumnsProportions.Add(new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Fill,
			});
			Padding = new Thickness(10);
			Widgets.Add(verticalStackPanel1);
			Widgets.Add(LevelInfo);
		}

		
		public TextButton HideFolders;
		public ComboBox SortMode;
		public ScrollViewer LevelScrollViewer;
		public TextButton Back;
		public Image HeartImg;
		public Label Title;
		public Label Artist;
		public Label Difficulty;
		public Label Subtitle;
		public Label Length;
		public Label Creator;
		public TextButton Play;
		public TextButton Mods;
		public TextButton LevelSettings;
		public Label ScoresHeader;
		public Grid ScoresContainer;
		public Label DetailsHeader;
		public Label Playcount;
		public VerticalStackPanel LevelInfo;
	}
}