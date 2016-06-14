using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace CADio.Mathematics.Numerical
{
    public class DeBoorSolverRecursive3D
    {
        private readonly DeBoorSolverRecursive1D[] _solvers;

        public DeBoorSolverRecursive3D(IList<Point3D> controlPoints, IList<double> knotPositions = null)
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

            _solvers = new[]
            {
                new DeBoorSolverRecursive1D(solverData[0], knotPositions),
                new DeBoorSolverRecursive1D(solverData[1], knotPositions),
                new DeBoorSolverRecursive1D(solverData[2], knotPositions),
            };
        }

        public Point3D Evaluate(double t, bool applyParameterCorrection = false)
        {
            return new Point3D(
                _solvers[0].Evaluate(t, applyParameterCorrection),
                _solvers[1].Evaluate(t, applyParameterCorrection),
                _solvers[2].Evaluate(t, applyParameterCorrection)
            );
        }

        public Vector3D Derivative(
            double t,
            bool applyParameterCorrection = true
            )
        {
            return new Vector3D(
                _solvers[0].Derivative(t, applyParameterCorrection),
                _solvers[1].Derivative(t, applyParameterCorrection),
                _solvers[2].Derivative(t, applyParameterCorrection)
            );
        }

    }
}