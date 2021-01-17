using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    /// <summary>
    /// Represents one piece of a piecewise function.
    /// </summary>
    public struct Piece
    {
        public PolynomialFunction Function { get; private set; }

        public double Start { get; private set; }

        public double End { get; private set; }

        /// <summary>
        /// Integral of the function between <c>Start</c> and <c>End</c>.
        /// </summary>
        public double Integral { get; private set; }

        public Piece(double x1, double x2, PolynomialFunction f)
        {
            Function = f;
            Start = x1;
            End = x2;
            Integral = 0;
            Integral = Function.IntegralBetween(Start, End);
        }
    }
}
