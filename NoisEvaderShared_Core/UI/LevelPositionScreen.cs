namespace NoisEvader.UI
{
    public partial class LevelPositionScreen
    {
        public LevelPositionScreen()
        {
            BuildUI();
        }

        public void Show()
        {
            NoisEvader.Desktop.Root = this;
        }
    }
}