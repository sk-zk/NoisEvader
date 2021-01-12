using System;

namespace NoisEvader_DesktopGL
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new NoisEvader.NoisEvader())
                game.Run();
        }
    }
}
