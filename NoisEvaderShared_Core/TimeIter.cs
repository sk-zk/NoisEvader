using System;
using System.Collections.Generic;

namespace NoisEvader
{
    public class TimeIter<T> // TODO Come up with a real name for this
    {
        public Func<T, double> TimeSelector { get; set; }

        public Action<TimeIterActionArgs<T>> Action { get; set; }

        private readonly List<T> list;

        public int CurrentIndex => idx;

        private int idx;

        public TimeIter(
                List<T> list,
                Func<T, double> timeSelector,
                Action<TimeIterActionArgs<T>> valueChangedAction)
        {
            this.list = list;
            TimeSelector = timeSelector;
            Action = valueChangedAction;
        }

        public void Update(LevelTime levelTime)
        {
            while (idx < list.Count 
                && levelTime.SongPosition >= TimeSelector.Invoke(list[idx]))
            {
                var time = TimeSelector.Invoke(list[idx]);
                var lateBy = levelTime.SongPosition - time;
                var args = new TimeIterActionArgs<T>()
                {
                    List = list,
                    Index = idx,
                    LevelTime = levelTime,
                    LateBy = lateBy,
                };
                Action.Invoke(args);
                idx++;
            }
        }

        public void Reset()
        {
            idx = 0;
        }
    }

    public class TimeIterActionArgs<T>
    {
        public List<T> List { get; set; }
        public int Index { get; set; }
        public LevelTime LevelTime { get; set; }
        public double LateBy { get; set; }
    }
}
