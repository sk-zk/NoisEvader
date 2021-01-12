using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NoisEvader.Replays
{
    public class ReplayRecorder
    {
        public Replay Replay { get; private set; }

        private double timeSinceLastUpdate;

        private double totalElapsed;

        public ReplayRecorder()
        {
            Replay = new Replay();
            timeSinceLastUpdate = 0;
        }

        public void Update(float elapsed)
        {
            totalElapsed += elapsed;
            timeSinceLastUpdate += elapsed;
        }

        public void AddPlayerPos(Vector2 position)
        {
            if (timeSinceLastUpdate > Replay.TickRate)
            {
                timeSinceLastUpdate -= Replay.TickRate;
                Replay.PlayerPositions.Add(new TsVal<Vector2>((float)totalElapsed, position));
            }
        }

        public void AddHit(InvincibilityType type)
        {
            Replay.Hits.Add(new TsVal<InvincibilityType>((float)totalElapsed, type));
        }

        public void AddHeart()
        {
            Replay.Hearts.Add((float)totalElapsed);
        }

        public void AddSlomo(bool slomo)
        {
            Replay.SlomoToggles.Add(new TsVal<bool>((float)totalElapsed, slomo));
        }

        public void Reset()
        {
            totalElapsed = 0;
            Replay.Clear();
        }
    }

}
