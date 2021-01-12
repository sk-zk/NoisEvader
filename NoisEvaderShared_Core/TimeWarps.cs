using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NoisEvader
{
    public class TimeWarps
    {
        public List<TsVal<float>> Nodes { get; set; }

        public TimeWarps(List<TsVal<float>> nodes)
        {
            Nodes = nodes;
        }

        public double GetAccurateWarpFactor(double prevT, double currentT)
        {
            double deltaT = currentT - prevT;

            if (deltaT == 0)
                return GetNaiveWarpFactor((float)prevT);

            if (prevT > Nodes[^1].Time)
                return Nodes[^1].Value;

            // find all nodes inbetween prev and current; as well the node before and after them
            // TODO: optimise this
            var prevNode = Nodes.LastOrDefault(x => x.Time <= prevT) ?? Nodes[0];
            var nextNode = Nodes.FirstOrDefault(x => x.Time >= currentT) ?? Nodes[^1];
            var inbetween = Nodes.Where(x => x.Time > prevT && x.Time < currentT).ToList();

            if (inbetween.Count == 0)
            {
                return GetAccurateWarp(prevNode, nextNode, prevT, currentT) / deltaT;
            }
            else
            {
                double result = 0;
                TsVal<float> n1 = prevNode;
                TsVal<float> n2;
                double t1 = prevT;
                double t2;
                for (int i = 0; i < inbetween.Count + 1; i++)
                {
                    if (i < inbetween.Count)
                    {
                        n2 = inbetween[i];
                        t2 = inbetween[i].Time;
                    }
                    else
                    {
                        n2 = nextNode;
                        t2 = currentT;
                    }
                    result += GetAccurateWarp(n1, n2, t1, t2);
                    n1 = n2;
                    t1 = t2;
                }
                return result / deltaT;
            }
        }

        private double GetAccurateWarp(TsVal<float> node1, TsVal<float> node2, double t1, double t2)
        {
            double duration = node2.Time - node1.Time;

            if (duration < float.Epsilon)
                return node2.Value;

            double progress1 = (t1 - node1.Time) / duration;
            double progress2 = (t2 - node1.Time) / duration;
            double val1 = MathHelper.Lerp(node1.Value, node2.Value, (float)progress1);
            double val2 = MathHelper.Lerp(node1.Value, node2.Value, (float)progress2);

            return LinearIntegralBetween(t1, t2, val1, val2);
        }

        private static double LinearIntegralBetween(double t1, double t2, double val1, double val2)
        {
            double deltaT = t2 - t1;
            double halfDeltaT = 0.5 * deltaT;
            return (halfDeltaT * val1) + (halfDeltaT * val2);
        }

        public float GetNaiveWarpFactor(float currentT)
        {
            // find first tw node ahead of the current position.
            // the current val will be between this and the previous node.
            TsVal<float> rateStart = null;
            TsVal<float> rateEnd = null;
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].Time >= currentT)
                {
                    rateEnd = Nodes[i];

                    if (i == 0)
                        rateStart = rateEnd;
                    else
                        rateStart = Nodes[i - 1];

                    break;
                }
            }
            // if we're beyond the last point, return the last point.
            if (rateStart is null && rateEnd is null)
                return Nodes[^1].Value;

            // interpolate tw.
            if (rateEnd.Time == rateStart.Time)
                return rateEnd.Value;
            var duration = rateEnd.Time - rateStart.Time;
            var progress = (currentT - rateStart.Time) / duration;
            return MathHelper.Lerp(rateStart.Value, rateEnd.Value, progress);
        }
    }
}
