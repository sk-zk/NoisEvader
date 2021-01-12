using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    /// <summary>
    /// The script of a level containing bullets, spinrate and timewarp.
    /// </summary>
    public class LevelScript
    {
        public List<TsVal<float>> TimeWarpNodes { get; set; }
        public List<TsVal<float>> SpinRateNodes { get; set; }
        public List<Shot> Shots { get; set; }
    }

}
