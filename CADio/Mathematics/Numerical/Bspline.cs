using System;
using System.Collections.Generic;

namespace CADio.Mathematics.Numerical
{
    public static class Bspline
    {
        public static IList<double> GetEquidistantKnots(int numControlPoints, 
            int degree)
        {
            var intervals = numControlPoints + degree;
            var knotPositions = new double[intervals + 1];

            for (var i = 0; i < knotPositions.Length; ++i)
                knotPositions[i] = (double)i / intervals;

            return knotPositions;
        }

        public static double EvaluateBspline(IList<double> controlPoints, 
            IList<double> knots, int degree, double t)
        {
            var interval = knots.Count - 2;
            for (; t < knots[interval]; --interval) ;

            var currentMultiplicity = 0;
            while (interval - currentMultiplicity > 0 &&
                Math.Abs(
                    t - knots[interval - currentMultiplicity]
                ) < 0.00001f)
                ++currentMultiplicity;

            var d = new double[degree - currentMultiplicity + 1, 
                degree - currentMultiplicity + 1];

            for (var i = 0; i <= degree - currentMultiplicity; ++i)
                d[0, i] = controlPoints[interval - degree + i];

            for (var k = 1; k <= degree - currentMultiplicity; ++k)
            {
                for (var i = k; i <= degree - currentMultiplicity; ++i)
                {
                    var knoti = i + interval - degree;
                    var alpha = (t - knots[knoti]) /
                        (knots[knoti+degree+1-k] - knots[knoti]);

                    d[k, i] = (1 - alpha)*d[k - 1, i - 1] + alpha*d[k - 1, i];
                }
            }

            return d[degree - currentMultiplicity, degree-currentMultiplicity];
        }
    }
}