using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using CADio.Mathematics.Interfaces;
using CADio.Mathematics.Numerical;
using MathNet.Numerics.LinearAlgebra.Double;

namespace CADio.Mathematics.Patches
{
    public class GregoryPatch : IParametricSurface
    {
        private readonly Point3D[] _cornerPoints = new Point3D[4];
        private readonly Vector3D[,] _cornerDerivatives = new Vector3D[4, 2];
        private readonly Vector3D[,] _cornerTwistVectors = new Vector3D[4, 2];

        public void SetCornerPoint(int id, Point3D point)
        {
            _cornerPoints[id] = point;
        }

        public Point3D GetCornerPoint(int id)
        {
            return _cornerPoints[id];
        }

        public void SetCornerDerivatives(
            int cornerId,
            Vector3D cornerDerivativeU,
            Vector3D cornerDerivativeV
            )
        {
            _cornerDerivatives[cornerId, 0] = cornerDerivativeU;
            _cornerDerivatives[cornerId, 1] = cornerDerivativeV;
        }

        public Vector3D GetCornerDerivativeU(int cornerId)
        {
            return _cornerDerivatives[cornerId, 0];
        }

        public Vector3D GetCornerDerivativeV(int cornerId)
        {
            return _cornerDerivatives[cornerId, 1];
        }

        public void SetCornerTwistVectors(
            int cornerId,
            Vector3D cornerTwistVectorUV,
            Vector3D cornerTwistVectorVU
            )
        {
            _cornerTwistVectors[cornerId, 0] = cornerTwistVectorUV;
            _cornerTwistVectors[cornerId, 1] = cornerTwistVectorVU;
        }

        public Vector3D GetCornerTwistVectorUV(int cornerId)
        {
            return _cornerTwistVectors[cornerId, 0];
        }

        public Vector3D GetCornerTwistVectorVU(int cornerId)
        {
            return _cornerTwistVectors[cornerId, 1];
        }

        public void ReshapeEdge(
            int edgeId, 
            IList<Point3D> boundaryCurve,
            Vector3D a0, 
            Vector3D b0,
            Vector3D a3, 
            Vector3D b3
            )
        {
            var boundaryCurveDerivative =
                BernsteinPolynomial.CalculateDerivative(boundaryCurve);

            var c0 = (Vector3D)BernsteinPolynomial.Evaluate3DPolynomial(
                boundaryCurveDerivative, 0.0
            );

            var c2 = (Vector3D)BernsteinPolynomial.Evaluate3DPolynomial(
                boundaryCurveDerivative, 1.0
            );

            var g0 = (a0 + b0)/2.0;
            var g2 = (a3 + b3)/2.0;
            var g1 = (g0 + g2)/2.0;

            var gPolynomial = new List<Point3D>()
            {
                (Point3D) g0,
                (Point3D) g1,
                (Point3D) g2
            };

            double k0, k1, h0, h1;
            CalculateTangentVectorFieldCoefficients(b0, g0, c0, out k0, out h0);
            CalculateTangentVectorFieldCoefficients(b3, g2, c2, out k1, out h1);

            Func<double, double> k = v => k0*(1 - v) + k1*v;
            Func<double, double> h = v => h0*(1 - v) + h1*v;
            Func<double, Vector3D> c = v =>
                (Vector3D) BernsteinPolynomial.Evaluate3DPolynomial(
                    boundaryCurveDerivative, v
                );
            Func<double, Vector3D> g = v =>
                (Vector3D) BernsteinPolynomial.Evaluate3DPolynomial(
                    gPolynomial, v
                );
            Func<double, Vector3D> d = v => k(v)*g(v) + h(v)*c(v);

            int v0 = 2, v1 = 0, derivativeIndex = 1;

            if (edgeId == 0)
            {
                v0 = 0;
                v1 = 1;
                derivativeIndex = 0;
            }
            else if (edgeId == 1)
            {
                v0 = 1;
                v1 = 3;
                derivativeIndex = 1;
            }
            else if (edgeId == 2)
            {
                v0 = 3;
                v1 = 2;
                derivativeIndex = 0;
            }

            _cornerPoints[v0] = boundaryCurve[0];
            _cornerPoints[v1] = boundaryCurve[3];
            _cornerDerivatives[v0, derivativeIndex] = d(0);
            _cornerDerivatives[v1, derivativeIndex] = d(1);
            _cornerTwistVectors[v0, 1 - derivativeIndex] = d(1.0/3.0);
            _cornerTwistVectors[v1, 1 - derivativeIndex] = d(2.0/3.0);
        }

        public Point3D Evaluate(double u, double v)
        {
            var corner = FindCorner(u, v);
            if (corner.HasValue)
                return _cornerPoints[corner.Value];

            var replacements = CalculateGregoryReplacements(u, v);
            var hu = GetHermiteVector(u);
            var hv = GetHermiteVector(v);

            var gx = BuildGeometryMatrix(replacements, (vec) => vec.X);
            var gy = BuildGeometryMatrix(replacements, (vec) => vec.Y);
            var gz = BuildGeometryMatrix(replacements, (vec) => vec.Z);

            var x = hu*gx*hv;
            var y = hu*gy*hv;
            var z = hu*gz*hv;

            return new Point3D(x, y, z);
        }

        protected int? FindCorner(
            double u, 
            double v, 
            double threshold = double.Epsilon
            )
        {
            var uEquals0 = MathHelpers.AlmostEqual(u, 0, threshold);
            var uEquals1 = MathHelpers.AlmostEqual(u, 1, threshold);
            var vEquals0 = MathHelpers.AlmostEqual(v, 0, threshold);
            var vEquals1 = MathHelpers.AlmostEqual(v, 1, threshold);

            if (uEquals0)
            {
                if (vEquals0) return 0;
                if (vEquals1) return 1;
            }

            if (uEquals1)
            {
                if (vEquals0) return 2;
                if (vEquals1) return 3;
            }

            return null;
        }

        protected Vector3D[] CalculateGregoryReplacements(double u, double v)
        {
            return new Vector3D[4]
            {
                (u*_cornerTwistVectors[0, 0]
                    + v*_cornerTwistVectors[0, 1]) /(u+v),
                ((1 - u)*_cornerTwistVectors[1, 0]
                    + v*_cornerTwistVectors[1, 1]) / (1 - u + v),
                (u*_cornerTwistVectors[2, 0]
                    + (1 - v)*_cornerTwistVectors[2, 1]) / (1+u-v),
                ((1 - u)*_cornerTwistVectors[3, 0]
                    + (1 - v)*_cornerTwistVectors[3, 1]) / (2 - u - v),
            };
        }

        protected DenseVector GetHermiteVector(double t)
        {
            var t2 = t*t;
            var t3 = t*t*t;
            return DenseVector.OfArray(new[]
            {
                2*t3 - 3*t2 + 1,
                3*t2 - 2*t3,
                t3 - 2*t2 + t,
                t3 - t2
            });
        }

        protected DenseMatrix BuildGeometryMatrix(
            Vector3D[] replacements,
            Func<Vector3D, double> componentExtractor
            )
        {
            var geometryMatrix = DenseMatrix.Create(4, 4, 0.0);
            for (var x = 0; x < 2; ++x)
            {
                for (var y = 0; y < 2; ++y)
                {
                    var corner = 2*y + x;
                    geometryMatrix[y, x] = 
                        componentExtractor((Vector3D)_cornerPoints[corner]);

                    geometryMatrix[2 + y, x] =
                        componentExtractor(_cornerDerivatives[corner, 0]);

                    geometryMatrix[y, 2 + x] =
                        componentExtractor(_cornerDerivatives[corner, 1]);

                    geometryMatrix[2 + y, 2 + x] =
                        componentExtractor(replacements[corner]);
                }
            }
            return geometryMatrix;
        }

        public static void CalculateTangentVectorFieldCoefficients(
            Vector3D b,
            Vector3D g,
            Vector3D c,
            out double k,
            out double h
            )
        {
            var A = MathNet.Numerics.LinearAlgebra.Matrix<double>
                .Build.DenseOfArray(new [,]
                {
                    {g.X, c.X},
                    {g.Y, c.Y},
                    {g.Z, c.Z}
                });
            var vec = MathNet.Numerics.LinearAlgebra.Vector<double>
                .Build.Dense(new [] {b.X, b.Y, b.Z});

            var solution = A.Solve(vec);

            k = solution[0];
            h = solution[1];
        }
    }
}