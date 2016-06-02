using System.Collections.Generic;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes.Dynamic;
using CADio.Helpers;
using CADio.Mathematics.Interfaces;
using CADio.Mathematics.Numerical;

namespace CADio.Mathematics.Patches
{
    public class BernsteinPatch : IParametricSurface
    {
        public Point3D[,] ControlPoints { get; } = new Point3D[4, 4];

        public Point3D Evaluate(double u, double v)
        {
            var subpoints = new List<Point3D>();
            for (var i = 0; i < 4; ++i)
            {
                var subpoint = BernsteinPolynomial.Evaluate3DPolynomial(
                    ArrayHelpers.GetRow(ControlPoints, i), u
                );

                subpoints.Add(subpoint);
            }

            return BernsteinPolynomial.Evaluate3DPolynomial(subpoints, v);
        }

        public Vector3D DerivativeU(double u, double v)
        {
            var subpoints = new List<Point3D>();
            for (var i = 0; i < 4; ++i)
            {
                var subpoint = BernsteinPolynomial.Evaluate3DPolynomial(
                    ArrayHelpers.GetColumn(ControlPoints, i), v
                );

                subpoints.Add(subpoint);
            }

            var derivative = BernsteinPolynomial.CalculateDerivative(subpoints);
            return (Vector3D)BernsteinPolynomial.Evaluate3DPolynomial(
                derivative, u
            );
        }
        
        public Vector3D DerivativeV(double u, double v)
        {
            var subpoints = new List<Point3D>();
            for (var i = 0; i < 4; ++i)
            {
                var subpoint = BernsteinPolynomial.Evaluate3DPolynomial(
                    ArrayHelpers.GetRow(ControlPoints, i), u
                );

                subpoints.Add(subpoint);
            }

            var derivative = BernsteinPolynomial.CalculateDerivative(subpoints);
            return (Vector3D)BernsteinPolynomial.Evaluate3DPolynomial(
                derivative, v
            );
        }
    }
}