using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoisEvader
{
    /// <summary>
    /// Represents a piecewise function.
    /// </summary>
    public class PiecewiseFunction
    {
        /// <summary>
        /// The pieces of the function.
        /// </summary>
        public List<Piece> Pieces { get; private set; }

        internal PiecewiseFunction()
        {
            Pieces = new List<Piece>();
        }

        public PiecewiseFunction(List<TsVal<float>> nodes)
        {
            Pieces = new List<Piece>(nodes.Count - 1);
            NodeListToPiecewiseLinearFunction(nodes);
        }

        public double F(double x)
        {
            if (x < Pieces[0].Start)
                throw new ArgumentOutOfRangeException(nameof(x));
            if (x > Pieces[^1].End)
                throw new ArgumentOutOfRangeException(nameof(x));

            // TODO optimise this
            var piece = Pieces.Last(p => p.Start <= x);
            return piece.Function.F(x);
        }

        /// <summary>
        /// Returns the integral of the function from 0 to <c>end</c>.
        /// </summary>
        /// <param name="end"></param>
        /// <returns></returns>
        public double IntegralTo(double end)
        {
            double sum = 0;
            for (int i = 0; i < Pieces.Count; i++)
            {
                if (Pieces[i].End > end)
                {
                    sum += Pieces[i].Function.IntegralBetween(Pieces[i].Start, end);
                    break;
                }
                else
                {
                    sum += Pieces[i].Integral;
                }
            }
            return sum;
        }

        /// <summary>
        /// Converts a timewarp / spinrate node list to a piecewise linear function.
        /// </summary>
        /// <param name="nodes"></param>
        private void NodeListToPiecewiseLinearFunction(List<TsVal<float>> nodes)
        {
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                double m, c;
                double deltaY = nodes[i + 1].Value - nodes[i].Value;
                if (deltaY == 0)
                {
                    m = 0;
                    c = nodes[i].Value;
                }
                else
                {
                    double deltaX = nodes[i + 1].Time - nodes[i].Time;
                    m = deltaY / deltaX;
                    c = nodes[i].Value - m * nodes[i].Time;
                }
                var f = new PolynomialFunction(c, m);
                Pieces.Add(new Piece(nodes[i].Time, nodes[i + 1].Time, f));
            }
        }

        public static PiecewiseFunction operator *(PiecewiseFunction f, PiecewiseFunction g)
        {
            List<double> limits = f.Pieces.Select(x => x.Start)
                .Union(g.Pieces.Select(x => x.Start))
                .ToList();

            var fEnd = f.Pieces[^1].End;
            var gEnd = f.Pieces[^1].End;
            if (!limits.Contains(fEnd))
                limits.Add(fEnd);
            if (fEnd != gEnd && !limits.Contains(gEnd))
                limits.Add(gEnd);

            limits.Sort();

            var h = new PiecewiseFunction();
            for (int i = 0; i < limits.Count - 1; i++)
            {
                var start = limits[i];
                var end = limits[i + 1];
                // TODO optimise this
                var piece1 = f.Pieces.Last(p => p.Start <= start);
                var piece2 = g.Pieces.Last(p => p.Start <= start);
                var multipliedFunc = piece1.Function * piece2.Function;
                var multipliedPiece = new Piece(start, end, multipliedFunc);
                h.Pieces.Add(multipliedPiece);
            }
            return h;
        }

    }
}
