using System;
using System.Linq;

namespace CADio.Mathematics.Numerical
{
    public class DeBoorSolver1D
    {
        private readonly double[] _deBoorPointsValues;
        private double[] _knotPositions;

        public DeBoorSolver1D(double[] deBoorPointsValues, double[] knotPositions = null)
        {
            _deBoorPointsValues = deBoorPointsValues;
            _knotPositions = knotPositions;
        }

        public double Evaluate(double t)
        {
            if (_deBoorPointsValues.Length < 2)
                throw new ApplicationException("Too few de Boor points passed.");

            if (_knotPositions == null)
                EvaluateEquidistantKnotPositions();

            t = CorrectTParameter(t, 3);

            return _deBoorPointsValues
                .Select((value, i) => EvaluateBsplineFunctionRecursively(3, i, t)*value)
                .Sum();
        }

        private double CorrectTParameter(double t, int degree)
        {
            var m = degree + _deBoorPointsValues.Length;
            var fixedInterval = _knotPositions[m - degree] - _knotPositions[degree];
            return (double) degree/m + t*fixedInterval;
        }

        public double EvaluateBsplineFunctionRecursively(int n, int i, double t)
        {
            if (n == 0)
            {
                if (t >= _knotPositions[i] && t < _knotPositions[i + 1])
                    return 1;
                return 0;
            }

            var left = (t - _knotPositions[i])/(_knotPositions[i + n] - _knotPositions[i])
                       *EvaluateBsplineFunctionRecursively(n - 1, i, t);
            var right = (_knotPositions[i + n + 1] - t)/(_knotPositions[i + n + 1] - _knotPositions[i + 1])
                        *EvaluateBsplineFunctionRecursively(n - 1, i + 1, t);

            return left + right;
        }

        private void EvaluateEquidistantKnotPositions()
        {
            const int degree = 3;
            var intervals = _deBoorPointsValues.Length + degree;
            var knotPositions = new double[intervals + 1];
            var dt = 1.0/intervals;

            for (var i = 0; i < knotPositions.Length; ++i)
                knotPositions[i] = (double)i/intervals;

            _knotPositions = knotPositions;
        }
    }
}