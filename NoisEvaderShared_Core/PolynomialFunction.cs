using System;
using System.Collections.Generic;
using System.Text;

namespace NoisEvader
{
    /// <summary>
    /// Represents a polynomial function.
    /// </summary>
    public struct PolynomialFunction
    {
        public double[] Coefficients { get; private set; }

        public int Degree => Coefficients.Length - 1;

        public PolynomialFunction(params double[] coefs)
        {
            Coefficients = coefs;
        }

        public double F(double x)
        {
            double result = Coefficients[0];
            for (int i = 1; i < Coefficients.Length; i++)
            {
                result += Math.Pow(x, i) * Coefficients[i];
            }
            return result;
        }

        public static PolynomialFunction operator *(PolynomialFunction f, PolynomialFunction g)
        {
            var coefF = f.Coefficients;
            var coefG = g.Coefficients;
            var m = coefF.Length;
            var n = g.Coefficients.Length;

            var coefH = new double[m + n - 1];

            for (int degF = 0; degF < m; degF++)
            {
                for (int degG = 0; degG < n; degG++)
                {
                    coefH[degF + degG] += coefF[degF] * coefG[degG];
                }
            }

            return new PolynomialFunction(coefH);
        }

        /// <summary>
        /// Returns the definite integral of the function.
        /// </summary>
        /// <param name="a">The lower limit.</param>
        /// <param name="b">The upper limit.</param>
        /// <returns></returns>
        public double IntegralBetween(double a, double b)
        {
            if (Degree == 1)
            {
                double deltaT = b - a;
                double halfDeltaT = 0.5 * deltaT;
                return halfDeltaT * F(a) + (halfDeltaT * F(b));
            }

            double sum = 0;
            for (int deg = 0; deg < Coefficients.Length; deg++)
            {
                var coef = Coefficients[deg];

                var defIntegral = coef * (
                        (Math.Pow(b, deg + 1) / (deg + 1))
                       - Math.Pow(a, deg + 1) / (deg + 1)
                    );

                sum += defIntegral;
            }
            return sum;
        }

        public override string ToString()
        {
            var polys = new List<string>();
            for (int deg = Coefficients.Length - 1; deg > 0; deg--)
            {
                if (Coefficients[deg] == 0 && deg != 0)
                    continue;

                polys.Add(PolyToString("x", Coefficients[deg], deg));
            }
            if (Coefficients[0] != 0 || polys.Count == 0)
            {
                // only add the constant coef if 1) it is nonzero, 
                // so we don't get a "+ 0" at the end,
                // or 2) it's the only coef, in which case
                // we print it even if it is zero
                polys.Add(Coefficients[0].ToString());
            }
            return string.Join(" + ", polys);
        }

        private static string PolyToString(string variable, double coef, int degree)
        {
            if (degree == 0)
                return coef.ToString();

            string output = "";

            if (coef == -1)
                output += "-";
            else if (coef != 1)
                output += coef.ToString();

            if (degree == 1)
                return output + variable;
            else
                return output + variable + "^" + degree.ToString();

        }
    }
}
