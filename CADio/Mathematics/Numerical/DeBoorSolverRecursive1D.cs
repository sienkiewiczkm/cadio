using System;
using System.Collections.Generic;
using System.Linq;

namespace CADio.Mathematics.Numerical
{
    public class DeBoorSolverRecursive1D
    {
        private readonly IList<double> _deBoorPointsValues;
        private IList<double> _knotPositions;

        public DeBoorSolverRecursive1D(IList<double> deBoorPointsValues, IList<double> knotPositions = null)
        {
            _deBoorPointsValues = deBoorPointsValues;
            _knotPositions = knotPositions;
        }

        public double Evaluate(double t, bool applyParameterCorrection = true)
        {
            if (_deBoorPointsValues.Count < 2)
                throw new ApplicationException("Too few de Boor points passed.");

            if (_knotPositions == null)
                EvaluateEquidistantKnotPositions();

            if (applyParameterCorrection)
                t = CorrectTParameter(t, 3);

            return Bspline.EvaluateBspline(_deBoorPointsValues, _knotPositions, 3, t);
            //return _deBoorPointsValues
            //.Select((value, i) => EvaluateBsplineFunctionRecursively(3, i, t, _knotPositions)*value)
            //.Sum();
        }

        private double CorrectTParameter(double t, int degree)
        {
            var m = degree + _deBoorPointsValues.Count;
            var fixedInterval = _knotPositions[m - degree] - _knotPositions[degree];
            return (double) degree/m + t*fixedInterval;
        }

        public static double EvaluateBsplineFunctionRecursively(int n, int i, double t, IList<double> knots)
        {
            if (n == 0)
            {
                if (t >= knots[i] && t < knots[i + 1])
                    return 1;
                return 0;
            }

            const double eps = 0.00001;
            var leftDenominator = knots[i + n] - knots[i];
            var left = 0.0;
            if (Math.Abs(leftDenominator) >= eps)
                left = (t - knots[i])/leftDenominator*EvaluateBsplineFunctionRecursively(n - 1, i, t, knots);

            var rightDenominator = knots[i + n + 1] - knots[i + 1];
            var right = 0.0;
            if (Math.Abs(rightDenominator) >= eps)
                right = (knots[i + n + 1] - t)/rightDenominator*EvaluateBsplineFunctionRecursively(n - 1, i + 1, t, knots);

            return left + right;
        }

        private void EvaluateEquidistantKnotPositions()
        {
            const int degree = 3;
            var intervals = _deBoorPointsValues.Count + degree;
            var knotPositions = new double[intervals + 1];

            for (var i = 0; i < knotPositions.Length; ++i)
                knotPositions[i] = (double)i/intervals;

            _knotPositions = knotPositions;
        }
    }
}