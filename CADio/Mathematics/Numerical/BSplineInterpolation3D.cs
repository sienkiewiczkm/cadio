using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Media3D;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace CADio.Mathematics.Numerical
{
    public struct BSplineInterpolationOutcome
    {
        public IList<Point3D> ControlPoints;
        public IList<double> Knots;
    };

    public static class BSplineInterpolation3D
    {
        public static BSplineInterpolationOutcome Interpolate(IList<Point3D> interpolationPoints, double a = 0.0001, double b = 0.9999, int degree = 3)
        {
            // coeff computing: http://www.cs.mtu.edu/~shene/COURSES/cs3621/NOTES/spline/B-spline/bspline-curve-coef.html
            // knot vector generation: http://www.cs.mtu.edu/~shene/COURSES/cs3621/NOTES/INT-APP/PARA-knot-generation.html
            // global curve interpolation: http://www.cs.mtu.edu/~shene/COURSES/cs3621/NOTES/INT-APP/CURVE-INT-global.html
            // http://www.cad.zju.edu.cn/home/zhx/GM/009/00-bsia.pdf

            var m = interpolationPoints.Count + degree;
            var d = CalculateChordLenghts(interpolationPoints);
            var t = CalculateParameters(interpolationPoints, a, b, d);

            for (var i = 0; i < t.Length - 1; ++i)
            {
                if (t[i] + double.Epsilon >= t[i + 1])
                    return new BSplineInterpolationOutcome
                    {
                        ControlPoints = new List<Point3D>(),
                        Knots = new List<double>(),
                    };
            }

            var u = CalculateKnots(interpolationPoints, degree, t);

            var m1 = 2;
            var m2 = 2;
            var N = new double[interpolationPoints.Count, m1+m2+1];

            for (var row = 0; row < interpolationPoints.Count; ++row)
            {
                var realstart = Math.Max(0, row - m1);
                var count = 1 + m2 + row - realstart;
                var maxcount = m1 + m2 + 1;
                var fixer = maxcount - count;

                for (var i = 0; i < count && realstart+i < interpolationPoints.Count; ++i)
                {
                    var col = realstart + i;
                    N[row, fixer+i] = DeBoorSolver1D.EvaluateBsplineFunctionRecursively(degree, col, t[row], u);
                }
            }

            var coords = new double[3][];
            DecomposePoint3DList(interpolationPoints, out coords[0], out coords[1], out coords[2]);

            var outPointsCoords = new double[3][];

            for (var i = 0; i < 3; ++i)
            {
                var mat = (double[,])N.Clone();
                double[,] up;
                int[] indx;
                double sign;

                var v = (double[])coords[i].Clone();
                LinearEquationSolver.bandec(mat, interpolationPoints.Count, m1, m2, out up, out indx, out sign);
                LinearEquationSolver.banbks(mat, interpolationPoints.Count, m1, m2, up, indx, v);
                outPointsCoords[i] = v;
            }

            var outPoints = new Point3D[interpolationPoints.Count];
            for (var i = 0; i < interpolationPoints.Count; ++i)
            {
                outPoints[i].X = outPointsCoords[0][i];
                outPoints[i].Y = outPointsCoords[1][i];
                outPoints[i].Z = outPointsCoords[2][i];
            }

            return new BSplineInterpolationOutcome {ControlPoints = outPoints, Knots = u};
        }

        public static void DecomposePoint3DList(IList<Point3D> interpolationPoints, out double[] x, out double[] y,
            out double[] z)
        {
            x = new double[interpolationPoints.Count];
            y = new double[interpolationPoints.Count];
            z = new double[interpolationPoints.Count];

            for (var i = 0; i < interpolationPoints.Count; ++i)
            {
                x[i] = interpolationPoints[i].X;
                y[i] = interpolationPoints[i].Y;
                z[i] = interpolationPoints[i].Z;
            }
        }

        private static double[] CalculateChordLenghts(IList<Point3D> interpolationPoints)
        {
            var d = new double[interpolationPoints.Count];
            d[0] = 0.0;
            for (var i = 1; i < interpolationPoints.Count; ++i)
                d[i] = d[i-1] + (interpolationPoints[i] - interpolationPoints[i - 1]).Length;
            return d;
        }

        private static double[] CalculateParameters(IList<Point3D> interpolationPoints, double a, double b, double[] d)
        {
            var totalChordLength = d[d.Length-1];
            var t = new double[interpolationPoints.Count];
            t[0] = a;
            t[interpolationPoints.Count - 1] = b;
            for (var i = 1; i < interpolationPoints.Count; ++i)
                t[i] = a + (d[i] / totalChordLength) * (b - a);
            return t;
        }

        private static double[] CalculateKnots(IList<Point3D> interpolationPoints, int degree, double[] t)
        {
            var u = new double[interpolationPoints.Count + degree + 1];
            for (var i = 0; i <= degree; ++i)
            {
                u[i] = 0.0f;
                u[u.Length - i - 1] = 1.0f;
            }

            for (var i = 1; i < interpolationPoints.Count - degree; ++i)
            {
                var forwardSum = 0.0;
                for (var j = i; j <= i + degree - 1; ++j)
                    forwardSum += t[j];
                u[degree + i] = (1.0/degree)*forwardSum;
            }
            return u;
        }
    }
}