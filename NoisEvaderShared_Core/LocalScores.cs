using System;
using System.Collections.Generic;

namespace NoisEvader
{
    public class LocalScores
    {
        public List<Score> Scores { get; set; }
            = new List<Score>();
        public uint PlayCount { get; set; }
        public DateTime LastPlayed { get; set; }
    }

}