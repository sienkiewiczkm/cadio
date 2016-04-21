using System;
using System.Collections.Generic;

namespace CADio.Mathematics.Numerical
{
    public static class LinearEquationSolver
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

        // void bandec(float **a, unsigned long n, int m1, int m2, float **al, unsigned long indx[], float* d)
        // a - compact input array
        // n - rows 
        // upper replaces a
        // lower al
        // index permutations
        // d free
        // a[n][m1+m2]
        public static void bandec(double[,] a, int n, int m1, int m2, out double[,] al, out int[] indx, out double d)
        {
            al = new double[a.GetLength(0), a.GetLength(1)];
            indx = new int[n];

            int i, j, k, l;
            int mm;
            double dum;

            mm = m1 + m2 + 1;
            l = m1;

            for (i = 1; i <= m1; i++)
            {
                for (j = m1 + 2 - i; j <= mm; j++)
                    a[i - 1, j - l - 1] = a[i - 1, j - 1];
                l--;
                for (j = mm - l; j <= mm; j++)
                    a[i - 1, j - 1] = 0.0;
            }

            d = 1.0;
            l = m1;

            for (k = 1; k <= n; k++)
            {
                dum = a[k - 1, 0];
                i = k;
                if (l < n) l++;
                for (j = k + 1; j <= l; j++)
                {
                    if (Math.Abs(a[j - 1, 0]) > Math.Abs(dum))
                    {
                        dum = a[j - 1, 0];
                        i = j;
                    }
                }

                indx[k - 1] = i;
                if (dum == 0.0)
                    a[k - 1, 0] = double.Epsilon;

                if (i != k)
                {
                    d = -d;
                    for (j = 1; j <= mm; j++)
                    {
                        var tmp = a[k - 1, j - 1];
                        a[k - 1, j - 1] = a[i - 1, j - 1];
                        a[i - 1, j - 1] = tmp;
                    }
                }

                for (i = k + 1; i <= l; i++)
                {
                    dum = a[i - 1, 0] / a[k - 1, 0];
                    al[k - 1, i - k - 1] = dum;
                    for (j = 2; j <= mm; j++)
                        a[i - 1, j - 2] = a[i - 1, j - 1] - dum * a[k - 1, j - 1];
                    a[i - 1, mm - 1] = 0.0;
                }
            }
        }

        public static void banbks(double[,] a, int n, int m1, int m2, double[,] al, int[] indx, double[] b)
        {
            const int ts = -1;
            int i, k, l;
            int mm;
            double dum;
            mm = m1 + m2 + 1;
            l = m1;

            for (k = 1; k <= n; k++)
            {
                i = indx[k + ts];
                if (i != k)
                {
                    var tmp = b[k + ts];
                    b[k + ts] = b[i + ts];
                    b[i + ts] = tmp;
                }
                if (l < n) l++;
                for (i = k + 1; i <= l; i++)
                    b[i + ts] -= al[k + ts, i - k + ts] * b[k + ts];
            }

            l = 1;
            for (i = n; i >= 1; i--)
            {
                dum = b[i + ts];
                for (k = 2; k <= l; k++)
                    dum -= a[i + ts, k + ts] * b[k + i - 1 + ts];
                b[i + ts] = dum / a[i + ts, 1 + ts];
                if (l < mm)
                    l++;
            }
        }
    }
}