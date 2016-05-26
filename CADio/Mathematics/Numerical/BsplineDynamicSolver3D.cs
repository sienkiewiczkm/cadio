using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace CADio.Mathematics.Numerical
{
    public class BsplineDynamicSolver3D
    {
        public BsplineDynamicSolver3D(IList<Point3D> controlPoints, IList<double> knotPositions = null)
        {
            var solverData = new[]
            {
                new double[controlPoints.Count],
                new double[controlPoints.Count],
                new double[controlPoints.Count],
            };

            for (var i = 0; i < controlPoints.Count; ++i)
            {
                var point = controlPoints[i];
                solverData[0][i] = point.X;
                solverData[1][i] = point.Y;
                solverData[2][i] = point.Z;
            }
        }
    }
}