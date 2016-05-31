using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using CADio.Mathematics.Interfaces;
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
    }
}