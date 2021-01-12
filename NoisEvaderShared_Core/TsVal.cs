using System.Diagnostics;

namespace NoisEvader
{
    /// <summary>
    /// Timestamped value. Name shortened because I would lose my mind otherwise.
    /// </summary>
    [DebuggerDisplay("{Time.ToString(\"0.##\")}\t{Value.ToString()}")]
    public class TsVal<T>
    {
        public float Time { get; set; }
        public T Value { get; set; }

        public TsVal() { }

        public TsVal(float time, T value)
        {
            Time = time;
            Value = value;
        }
    }
}
