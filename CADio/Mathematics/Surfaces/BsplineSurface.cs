using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using CADio.Mathematics.Interfaces;
using CADio.Mathematics.Numerical;
using CADio.Mathematics.Trimming;

namespace CADio.Mathematics.Surfaces
{
    public class BsplineSurface :
        IParametricSurface
    {
        public int SegmentsU { get; set; }
        public int SegmentsV { get; set; }
        public List<Point3D> ControlPoints { get; set; }

        public ISurfaceTrimmer Trimmer { get; set; }

        public ParametrisationBoundaries ParametrisationBoundaries
        {
            get
            {
                var controlPointsRows = ControlPoints.Count/(3+SegmentsU);
                var notLooped = controlPointsRows == (3+SegmentsV);
                return new ParametrisationBoundaries()
                {
                    AllowTraversalV = !notLooped
                };
            }
        }

        public Point3D Evaluate(double u, double v)
        {
            var recalculatedRowPoints = new List<Point3D>();

            var dataRows = ControlPoints.Count/(3 + SegmentsU);
            for (var row = 0; row < SegmentsV + 3; ++row)
            {
                var rowControlPoints = new List<Point3D>();
                for (var column = 0; column < SegmentsU + 3; ++column)
                {
                    var index = (3 + SegmentsU)*(row%dataRows) + column;
                    rowControlPoints.Add(ControlPoints[index]);
                }

                var solver = new DeBoorSolverRecursive3D(rowControlPoints);
                var calculatedPoint = solver.Evaluate(u, true);
                recalculatedRowPoints.Add(calculatedPoint);
            }

            var vSolver = new DeBoorSolverRecursive3D(recalculatedRowPoints);
            return vSolver.Evaluate(v, true);
        }

        public Vector3D Derivative(
            double u, 
            double v, 
            DerivativeParameter parameter
            )
        {
            switch (parameter)
            {
                case DerivativeParameter.U:
                    return DerivativeU(u, v);
                case DerivativeParameter.V:
                    return DerivativeV(u, v);
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(parameter), 
                        parameter, 
                        null
                    );
            }
        }

        public Vector3D DerivativeU(double u, double v)
        {
            var recalculatedRowPoints = new List<Point3D>();

            var dataRows = ControlPoints.Count/(3 + SegmentsU);
            for (var column = 0; column < SegmentsU + 3; ++column)
            {
                var rowControlPoints = new List<Point3D>();
                for (var row = 0; row < SegmentsV + 3; ++row)
                {
                    var index = (3 + SegmentsU)*(row%dataRows) + column;
                    rowControlPoints.Add(ControlPoints[index]);
                }

                var solver = new DeBoorSolverRecursive3D(rowControlPoints);
                var calculatedPoint = solver.Evaluate(v, true);
                recalculatedRowPoints.Add(calculatedPoint);
            }

            var uSolver = new DeBoorSolverRecursive3D(recalculatedRowPoints);
            return uSolver.Derivative(u, true);
        }

        public Vector3D DerivativeV(double u, double v)
        {
            var recalculatedRowPoints = new List<Point3D>();

            var dataRows = ControlPoints.Count/(3 + SegmentsU);
            for (var row = 0; row < SegmentsV + 3; ++row)
            {
                var rowControlPoints = new List<Point3D>();
                for (var column = 0; column < SegmentsU + 3; ++column)
                {
                    var index = (3 + SegmentsU)*(row%dataRows) + column;
                    rowControlPoints.Add(ControlPoints[index]);
                }

                var solver = new DeBoorSolverRecursive3D(rowControlPoints);
                var calculatedPoint = solver.Evaluate(u, true);
                recalculatedRowPoints.Add(calculatedPoint);
            }

            var vSolver = new DeBoorSolverRecursive3D(recalculatedRowPoints);
            return vSolver.Derivative(v, true);
        }
    }
}