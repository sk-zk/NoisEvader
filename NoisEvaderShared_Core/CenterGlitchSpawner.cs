using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    class CenterGlitchSpawner : Spawner
    {
        protected override void CalcPosition(ArenaCircle arena)
        {
            // do nothing
        }

        protected override void UpdateAngle(float spawnerSpeed)
        {
            // do nothing
        }

        protected override void UpdateFlares(LevelTime levelTime)
        {
            for (int i = 0; i < ActiveFlares.Count; i++)
            {
                if (ActiveFlares[i].HasFadedOut)
                {
                    ActiveFlares.RemoveAt(i);
                    i--;
                    continue;
                }
                ActiveFlares[i].Update(levelTime, Angle - MathHelper.ToRadians(90));
            }
        }
    }
}
