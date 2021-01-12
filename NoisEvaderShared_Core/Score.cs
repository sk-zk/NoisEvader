using System;

namespace NoisEvader
{
    public class Score
    {
        public string XmlHash { get; set; }
        public DateTime Time { get; set; }
        public float Percent { get; set; }
        public uint TotalHits { get; set; }
        public Mod Mod { get; set; }
        public bool HeartGotten { get; set; }

        public string ReplayFile { get; set; }
    }

}