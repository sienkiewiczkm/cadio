using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes.Dynamic;
using CADio.Helpers;
using CADio.Mathematics.Interfaces;
using CADio.Mathematics.Numerical;
using CADio.Mathematics.Proxies;
using CADio.Mathematics.Trimming;

namespace CADio.Mathematics.Patches
{
    public class BernsteinPatch : IParametricSurface
    {
        public Point3D[,] ControlPoints { get; } = new Point3D[4, 4];

        public ISurfaceTrimmer Trimmer { get; set; }

        public ParametrisationBoundaries ParametrisationBoundaries
            => new ParametrisationBoundaries();

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

        public Point3D Evaluate(Parametrisation parametrisation)
        {
            return Evaluate(parametrisation.U, parametrisation.V);
        }

        public Vector3D Derivative(
            double u, double v, 
            DerivativeParameter parameter
            )
        {
            double firstParameter, secondParameter;
            Func<int, Point3D[]> generator;

            switch (parameter)
            {
                case DerivativeParameter.U:
                    generator = (i) => ArrayHelpers.GetColumn(ControlPoints, i);
                    firstParameter = v;
                    secondParameter = u;
                    break;
                case DerivativeParameter.V:
                    generator = (i) => ArrayHelpers.GetRow(ControlPoints, i);
                    firstParameter = u;
                    secondParameter = v;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(parameter), 
                        parameter, 
                        null
                    );
            }

            var subpoints = new List<Point3D>();
            for (var i = 0; i < 4; ++i)
            {
                subpoints.Add(BernsteinPolynomial.Evaluate3DPolynomial(
                    generator(i), 
                    firstParameter
                ));
            }

            var derivative = BernsteinPolynomial.CalculateDerivative(subpoints);
            return (Vector3D) BernsteinPolynomial.Evaluate3DPolynomial(
                derivative,
                secondParameter
            );
        }

        public SurfaceReparametrisationFlip CreateReparametrisationUDirection(
            int startColumn,
            int startRow,
            int endColumn,
            int endRow
            )
        {
            if (!(startRow == 0 || startRow == 3 || endRow == 0 || endRow == 3))
                throw new ArgumentException(
                    "Specified points should be corner points"
                );

            bool flipU = false, flipV = false, flipUV = false;

            if (startRow == endRow && startRow == 3)
                flipV = true;

            if (startColumn == endColumn)
            {
                if (startColumn == 3)
                    flipU = true;
                flipUV = true;
            }

            if (startColumn > endColumn)
                flipU = true;
            if (startRow > endRow)
                flipV = true;

            return new SurfaceReparametrisationFlip()
            {
                Surface = this,
                FlipU = flipU,
                FlipV = flipV,
                FlipUV = flipUV,
            };
        }
    }
}