using System.Collections.Generic;

namespace CADio.Mathematics.Numerical
{
    public static class TridiagonalLinearEquationSolver
    {
        public static IList<double> SolveTDMA(double[,] matrix, double[] inp)
        {
            var n = matrix.GetLength(0);
            var a = new double[n];
            var b = new double[n];
            var c = new double[n];
            var d = (double[])inp.Clone();
            var x = new double[d.Length];

            for (var i = 0; i < n; ++i)
            {
                b[i] = matrix[i, i];
                if (i > 0)
                    a[i] = matrix[i, i - 1];
                if (i < n - 1)
                    c[i] = matrix[i, i + 1];
            }

            c[0] = c[0]/b[0];
            d[0] = d[0]/b[0];

            for (var i = 1; i < n; ++i)
            {
                var m = 1.0/(b[i] - a[i]*c[i - 1]);
                c[i] = m*c[i];
                d[i] = m*(d[i] - a[i]*d[i - 1]);
            }

            x[n - 1] = d[n - 1];
            for (var i = n - 2; i >= 0; --i)
            {
                x[i] = d[i] - c[i]*d[i + 1];
            }

            return x;
        }
    }
}