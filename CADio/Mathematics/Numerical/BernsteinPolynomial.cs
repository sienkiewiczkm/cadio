using System.Collections.Generic;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes.Dynamic;

namespace CADio.Mathematics.Numerical
{
    public class BernsteinPolynomial
    {
        public static IList<Point3D> CalculateDerivative(IList<Point3D> control)
        {
            var degree = control.Count - 1;
            var newControl = new List<Point3D>();
            for (var i = 0; i < degree; ++i)
                newControl.Add((Point3D)(degree*(control[i + 1] - control[i])));
            return newControl;
        }

        public static Point3D Evaluate3DPolynomial(
            IList<Point3D> control, 
            double parameter
            )
        {
            var solver = new DeCastlejauSolver(
                BezierCurveC0.FillBernsteinCoordinatesArray(
                    control, 
                    control.Count - 1, 
                    0
                )
            );
            return MathHelpers.MakePoint3D(solver.Evaluate(parameter));
        }
    }
}