using System;
using System.Collections.Generic;

namespace NoisEvader.UI
{
    using ComparerFunction = Func<CachedLevelData, CachedLevelData, int>;

    public class LevelComparer : IComparer<CachedLevelData>
    {
        public SortType Sort { get; set; } = SortType.Title;

        private const int Descending = -1;

        private static ComparerFunction titleComparer =
             (x, y) => string.Compare(x.Info.Title, y.Info.Title, true);

        private static ComparerFunction artistComparer =
            (x, y) => string.Compare(x.Info.Artist, y.Info.Artist, true);

        private static ComparerFunction difficultyComparer =
             (x, y) => Comparer<int>.Default.Compare(x.Info.Difficulty, y.Info.Difficulty);

        private static ComparerFunction durationComparer =
             (x, y) => TimeSpan.Compare(x.AudioDuration ?? TimeSpan.Zero, y.AudioDuration ?? TimeSpan.Zero);

        private static ComparerFunction creatorComparer =
                (x, y) => string.Compare(x.Info.Designer, y.Info.Designer, true);

        private static ComparerFunction playsComparer =
            (x, y) => Comparer<int>.Default.Compare(x.Playcount, y.Playcount) * Descending;

        private static ComparerFunction scoreComparer =
            (x, y) => Comparer<float>.Default.Compare(x.BestScore ?? -1f, y.BestScore ?? -1f) * Descending;

        public class SortDict : Dictionary<SortType, ComparerFunction[]> { }
        private static readonly SortDict Sorts = new SortDict()
        {
            { SortType.Title,
                new[] { titleComparer, artistComparer, difficultyComparer } },
            { SortType.Artist,
                new[] { artistComparer, titleComparer, difficultyComparer } },
            { SortType.Difficulty,
                new[] { difficultyComparer, titleComparer, artistComparer} },
            { SortType.Duration,
                new[] { durationComparer, titleComparer, artistComparer, difficultyComparer } },
            { SortType.Creator,
                new[] { creatorComparer, titleComparer, artistComparer, difficultyComparer } },
            { SortType.Plays,
                new[] { playsComparer, titleComparer, artistComparer, difficultyComparer } },
            { SortType.Score,
                new[] { scoreComparer, titleComparer, artistComparer, difficultyComparer } },
        };

        public int Compare(CachedLevelData x, CachedLevelData y)
        {
            foreach (var c in Sorts[Sort])
            {
                var result = c.Invoke(x, y);
                if (result != 0)
                    return result;
            }

            return 0;
        }
    }
}
