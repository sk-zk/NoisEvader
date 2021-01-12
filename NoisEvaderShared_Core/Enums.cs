using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    public enum BulletType
    {
        Normal,
        Normal2,
        Homing,
        Bubble,
        Hug,
        Heart,
        Error
    }

    public enum ShotType
    {
        Normal, Wave, Stream, Burst
    }

    public enum Aim
    {
        Center, Player // mid, pl
    }

    public enum ScaleMode
    {
        Fit,
        Flash
    }

    public enum WindowType
    {
        Windowed,
        Fullscreen,
        Borderless
    }
}
