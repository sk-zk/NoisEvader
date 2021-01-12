using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    public static class Util
    {
        /// <summary>
        /// Returns the color the score circle in the center should have.
        /// </summary>
        public static Color GetActualScoreColor(Color color)
            => Color.Lerp(color, Color.Transparent, 0.75f);

        /// <summary>
        /// Gets the spawner that corresponds to an enemy index of a Shot.
        /// </summary>
        /// <param name="spawners">The list of spawners.</param>
        /// <param name="shotIdx">The enemy index of the Shot.</param>
        public static Spawner GetSpawnerFromShotIndex(List<Spawner> spawners, int shotIdx)
        {
            var maxIdx = spawners.Count - 1;
            var spawnerIdx = Math.Min(shotIdx, maxIdx);
            return spawners[spawnerIdx];
        }
    }
}
