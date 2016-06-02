using System.Collections.Generic;

namespace CADio.Mathematics.Numerical
{
    public class DeCastlejauSolver
    {
        private readonly double[,] _polynomialCoordinates;

        /// <param name="polynomialCoordinates">
        ///     NxM array, where N is the control points amount, M is the dimension of point
        /// </param>
        public DeCastlejauSolver(double[,] polynomialCoordinates)
        {
            _polynomialCoordinates = polynomialCoordinates;
        }

        public double[] Evaluate(double t)
        {
            var controlPointsNum = _polynomialCoordinates.GetLength(0);
            var dimension = _polynomialCoordinates.GetLength(1);
            var output = new double[dimension];
            var temp = new double[controlPointsNum];
            var u = 1 - t;

            for (var i = 0; i < dimension; ++i)
            {
                for (var j = 0; j < controlPointsNum; ++j)
                    temp[j] = _polynomialCoordinates[j, i];

                for (var j = controlPointsNum - 1; j >= 1; --j)
                    for (var k = 0; k < j; ++k)
                        temp[k] = u*temp[k] + t*temp[k + 1];

                output[i] = temp[0];
            }

            return output;
        }

        public double[] EvaluateWithSubdivide(
            double t,
            out double[,] left,
            out double[,] right
            )
        {
            var controlPointsNum = _polynomialCoordinates.GetLength(0);
            var dimension = _polynomialCoordinates.GetLength(1);
            var output = new double[dimension];
            var temp = new double[controlPointsNum];
            var u = 1 - t;

            left = new double[controlPointsNum, dimension];
            right = new double[controlPointsNum, dimension];

            for (var i = 0; i < dimension; ++i)
            {
                for (var j = 0; j < controlPointsNum; ++j)
                    temp[j] = _polynomialCoordinates[j, i];

                left[0, i] = temp[0];
                right[controlPointsNum - 1, i] = temp[controlPointsNum - 1];

                for (var j = controlPointsNum - 1; j >= 1; --j)
                {
                    for (var k = 0; k < j; ++k)
                        temp[k] = u*temp[k] + t*temp[k + 1];

                    left[controlPointsNum - j, i] = temp[0];
                    right[j - 1, i] = temp[j - 1];
                }

                output[i] = temp[0];
            }

            return output;
        }
    }
}