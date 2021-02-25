using Microsoft.Xna.Framework;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NoisEvader.UI
{
    class LevelBox : VerticalStackPanel
    {
        public CachedLevelData SelectedLevel { get; private set; }
        public event EventHandler<CachedLevelData> LevelSelected;

        public string LevelFolder { get; set; }

        private SortType sort = SortType.Title;
        public SortType Sort
        {
            get => sort;
            set
            {
                sort = value;
                SortLevels();
            }
        }

        private bool hideFolders = false;
        public bool HideFolders
        {
            get => hideFolders;
            set
            {
                hideFolders = value;
                FolderPanel.Visible = !value;
            }
        }

        public bool FirstLoadDone { get; private set; } = false;

        private readonly VerticalStackPanel LoadingMsgPanel;
        private readonly VerticalStackPanel FolderPanel;
        private readonly VerticalStackPanel LevelPanel;

        private List<CachedLevelData> levels = new List<CachedLevelData>();
        private List<string> folders = new List<string>();

        private LevelBoxButton selectedItem;
        // cache previous item to work around myra's naive doubleclick event
        // which fires even if you clicked on two different controls.
        private LevelBoxButton prevSelectedItem;

        private bool loadLevelFolderOnUpdate = false;

        private const string RootPath = "/";
        private const string ParentUiString = "..";
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public LevelBox()
        {
            Width = 530;
            DefaultProportion = new Proportion(ProportionType.Auto);

            LoadingMsgPanel = new VerticalStackPanel();
            var loadingMsg = new Label();
            loadingMsg.Text = "loading";
            loadingMsg.HorizontalAlignment = HorizontalAlignment.Center;
            LoadingMsgPanel.Widgets.Add(loadingMsg);

            FolderPanel = new VerticalStackPanel()
            {
                Spacing = 12,
            };
            Widgets.Add(FolderPanel);

            LevelPanel = new VerticalStackPanel()
            {
                Margin = new Thickness(0, 12, 0, 0),
                Spacing = 12,
            };
            Widgets.Add(LevelPanel);
        }

        public void Update()
        {
            if (loadLevelFolderOnUpdate)
            {
                LoadLevels(LevelFolder);
                loadLevelFolderOnUpdate = false;
            }
        }

        public void FirstLoad()
        {
            LoadLevels(LevelFolder);
            FirstLoadDone = true;
        }

        private void LoadLevels(string path)
        {
            logger.Info("Loading levels in {path}", path);

            Enabled = false;

            FolderPanel.Visible = false;
            FolderPanel.Widgets.Clear();
            folders.Clear();
            LevelPanel.Visible = false;
            LevelPanel.Widgets.Clear();
            levels.Clear();
            Widgets.Insert(0, LoadingMsgPanel);
            (Parent as ScrollViewer).ScrollPosition = Point.Zero;

            prevSelectedItem = null;

            Task.Run(() =>
            {
                // load folders to ui
                folders.Add(ParentUiString);
                var enumerationOptions = new EnumerationOptions()
                {
                    IgnoreInaccessible = true,
                };

                foreach (var folder in Directory.EnumerateDirectories(path, "*", enumerationOptions))
                    folders.Add(folder);

                PopulateFolders();

                foreach (var file in Directory.EnumerateFiles(path, "*.xml", SearchOption.TopDirectoryOnly))
                {
                    CachedLevelData level;
                    try
                    {
                        level = SoundodgerLevel.LoadInfoOnly(file);
                        levels.Add(level);
                    }
                    catch (XmlException xmlEx)
                    {
                        logger.Error(xmlEx, "Failed to load {xml}", file);
                        continue;
                    }
                }

                var cmp = new LevelComparer();
                cmp.Sort = Sort;
                levels.Sort(cmp);

                PopulateLevels();

                Enabled = true;
                Widgets.Remove(LoadingMsgPanel);
                FolderPanel.Visible = !HideFolders;
                LevelPanel.Visible = true;
            });
        }

        private void SortLevels()
        {
            var selected = SelectedLevel;
            var comparer = new LevelComparer();
            comparer.Sort = Sort;
            levels = levels.OrderBy(x => x, comparer).ToList();
            Repopulate();
            if (selected != null)
            {
                var newIdxOfSelected = levels.IndexOf(selected);
                if (newIdxOfSelected == -1)
                {
                    logger.Warn("selected level disappeared after sorting?");
                }
                else
                {
                    Select((LevelBoxButton)LevelPanel.Widgets[newIdxOfSelected]);
                    ScrollToLevel(newIdxOfSelected);
                }
            }
        }

        private void Repopulate()
        {
            FolderPanel.Widgets.Clear();
            LevelPanel.Widgets.Clear();
            prevSelectedItem = null;
            PopulateFolders();
            PopulateLevels();
        }

        private void ScrollToLevel(int itemIndex)
        {
            ScrollViewer parent = Parent as ScrollViewer;

            // calculate Y position of the item
            int scrollPos = 0;
            if (!HideFolders)
                scrollPos += FolderPanel.Measure(Point.Zero).Y;

            for (int i = 0; i < itemIndex; i++)
                scrollPos += LevelPanel.Spacing + LevelPanel.Widgets[i].Measure(Point.Zero).Y;

            // center item in box
            scrollPos -= (parent.ActualBounds.Height / 2)
                - LevelPanel.Widgets[itemIndex].Measure(Point.Zero).Y / 2;

            // clamp scroll at the edges
            scrollPos = MathHelper.Clamp(scrollPos, 0, parent.ScrollMaximum.Y);

            parent.ScrollPosition = new Point(0, scrollPos);
        }

        private void PopulateFolders()
        {
            if (LevelFolder == RootPath)
            {
                var drives = Directory.GetLogicalDrives();
                for (int i = 0; i < drives.Length; i++)
                {
                    AddFolder(drives[i], drives[i]);
                }
                return;
            }

            for (int i = 0; i < folders.Count; i++)
            {
                string folder = folders[i];
                if (folder == ParentUiString)
                {
                    var parentInfo = Directory.GetParent(LevelFolder);
                    var parent = (parentInfo is null)
                        ? RootPath
                        : parentInfo.FullName;
                    AddFolder(parent, ParentUiString);
                }
                else
                {
                    AddFolder(folder, Path.GetFileName(folder));
                }
            }

            void AddFolder(string folder, string display)
            {
                var btn = new LevelBoxFolder(display);
                btn.MouseEntered += Grid_MouseEntered;
                btn.MouseLeft += Grid_MouseLeft;
                btn.TouchDown += (_, _) => OnFolderClicked(btn);
                btn.TouchDoubleClick += (_, _) =>
                {
                    if (prevSelectedItem == selectedItem)
                        OnFolderDoubleClicked(folder);
                };
                FolderPanel.Widgets.Add(btn);
            }
        }

        private void OnFolderClicked(LevelBoxFolder folder)
        {
            Deselect(selectedItem);
            Select(folder);
        }

        private void OnFolderDoubleClicked(string folder)
        {
            LevelFolder = folder;
            loadLevelFolderOnUpdate = true;
        }

        private void PopulateLevels()
        {
            foreach (var level in levels)
                AddLevel(level);
        }

        private void AddLevel(CachedLevelData level)
        {
            var btn = CreateLevelButton(level);
            LevelPanel.Widgets.Add(btn);
        }

        private LevelBoxLevel CreateLevelButton(CachedLevelData level)
        {
            var btn = new LevelBoxLevel(level);
            btn.MouseEntered += Grid_MouseEntered;
            btn.MouseLeft += Grid_MouseLeft;
            btn.TouchDown += (_, _) => OnLevelClicked(btn, level);
            return btn;
        }

        private void Grid_MouseLeft(object sender, EventArgs e)
        {
            if (sender != selectedItem)
                (sender as Grid).Background = LevelBoxButton.UnselectedBackground;
        }

        private void Grid_MouseEntered(object sender, EventArgs e)
        {
            if (sender != selectedItem)
                (sender as Grid).Background = LevelBoxButton.HoverBackground;
        }

        private void OnLevelClicked(LevelBoxLevel sender, CachedLevelData level)
        {
            Deselect(selectedItem);
            Select(sender);
            SelectedLevel = level;
            LevelSelected?.Invoke(this, level);
        }

        private void Select(LevelBoxButton button)
        {
            button.Background = LevelBoxButton.SelectedBackground;
            prevSelectedItem = selectedItem;
            selectedItem = button;
        }

        private void Deselect(LevelBoxButton button)
        {
            if (button is null)
                return;
            button.Background = LevelBoxButton.UnselectedBackground;
        }

        public void UpdateSelected()
        {
            if (selectedItem != null) 
            {
                var selectedIdx = LevelPanel.Widgets.IndexOf(selectedItem);
                selectedItem = CreateLevelButton(SelectedLevel);
                LevelPanel.Widgets[selectedIdx] = selectedItem;
            }
        }

        public void WindowResized()
        {
            // [HACK] Fixes labels getting 0 width after resizing the window
            Repopulate();
        }
    }
}
