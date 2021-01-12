using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    public struct LevelTime
    {
        /// <summary>
        /// Game time. 
        /// Note that Elapsed is actually the elapsed song time rather than real time.
        /// </summary>
        public GameTime GameTime { get; set; }

        public double SongPosition { get; set; }

        public double GameSpeed { get; set; }

        public double TimeWarp { get; set; }

        public double SpinRate { get; set; }

        /// <summary>
        /// Elapsed milliseconds. Note that this is already multiplied by
        /// game speed while the song is playing.
        /// </summary>
        public double ElapsedMs => GameTime.ElapsedGameTime.TotalMilliseconds;

        public double LevelElapsedMs => ElapsedMs * TimeWarp;


    }
}
