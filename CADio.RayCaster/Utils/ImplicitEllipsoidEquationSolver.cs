using System;
using CADio.Mathematics;

namespace CADio.RayCaster.Utils
{
    public static class ImplicitEllipsoidEquationSolver
    {
        private static bool IsApproximatelyEqual(double x, double y, double acceptableVariance = 1E-15)
        {
            var variance = x > y ? x - y : y - x;
            return variance < acceptableVariance;
        }

        public static double? SolveZ(Vector4D v, Matrix4X4 mat)
        {
            var A = mat[2, 2];
            var B = (mat[2, 0] + mat[0, 2]) * v.X + (mat[2, 1] + mat[1, 2]) * v.Y + (mat[3, 2] + mat[2, 3]) * v.W;
            var C = v.X * v.X * mat[0, 0] + v.Y * v.Y * mat[1, 1] + v.W * v.W * mat[3, 3]
                    + (mat[1, 0] + mat[0, 1]) * v.X * v.Y
                    + (mat[3, 0] + mat[0, 3]) * v.X * v.W
                    + (mat[3, 1] + mat[1, 3]) * v.Y * v.W;

            if (IsApproximatelyEqual(A, 0) && IsApproximatelyEqual(B, 0))
            {
                // constant equation C = 0
                return null;
            }

            if (IsApproximatelyEqual(A, 0))
            {
                // linear equation Bx+C=0 => x=-C/B
                return -C / B;
            }

            // quadratic equation Ax+By+C=0 A != 0
            var delta = B * B - 4 * A * C;
            if (delta < 0.0)
                return null;

            // return smaller solution
            return Math.Min(
                (-B - Math.Sqrt(delta)) / (2 * A),
                (-B + Math.Sqrt(delta)) / (2 * A)
            );
        }

        public static Vector4D CalculateNormal(Vector4D point, Matrix4X4 mat)
        {
            return new Vector4D(
                DerivativeX(point, mat),
                DerivativeY(point, mat),
                DerivativeZ(point, mat),
                0.0
            ).Normalize();
        }

        private static double DerivativeX(Vector4D v, Matrix4X4 mat)
        {
            return 2.0*v.X*mat[0, 0]
                   + (mat[1, 0] + mat[0, 1])*v.Y
                   + (mat[2, 0] + mat[0, 2])*v.Z
                   + (mat[3, 0] + mat[0, 3])*v.W;
        }

        private static double DerivativeY(Vector4D v, Matrix4X4 mat)
        {
            return 2.0*v.Y*mat[1, 1]
                   + (mat[1, 0] + mat[0, 1])*v.X
                   + (mat[2, 1] + mat[1, 2])*v.Z
                   + (mat[3, 1] + mat[1, 3])*v.W;
        }

        private static double DerivativeZ(Vector4D v, Matrix4X4 mat)
        {
            return 2.0*mat[2, 2]*v.Z
                   + (mat[2, 0] + mat[0, 2])*v.X
                   + (mat[2, 1] + mat[1, 2])*v.Y
                   + (mat[3, 2] + mat[2, 3])*v.W;
        }
    }
}