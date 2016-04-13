using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.Mathematics;
using CADio.Mathematics.Numerical;

namespace CADio.Geometry.Shapes.Dynamic
{
    public class BezierCurveC0 : IDynamicShape
    {
        public string Name => "Bezier Curve C0";

        public IList<Vertex> Vertices { get; private set; }
        public IList<IndexedLine> Lines { get; private set; }
        public IList<Vertex> MarkerPoints => new List<Vertex>();
        public IList<Vertex> RawPoints { get; private set; }
        public bool IsPolygonRenderingEnabled { get; set; }

        public Control GetEditorControl() => null;

        public IList<Point3D> ControlPoints = new List<Point3D>(); 

        public void UpdateGeometry(Func<Point3D, Point3D, double> estimateScreenSpaceDistanceWithoutClip,
            Predicate<Point3D> isInsideProjectiveCubePredicate)
        {
            const int targetDegree = 3;

            var rawPointsList = SampleBezierCurveC0(ControlPoints, estimateScreenSpaceDistanceWithoutClip, targetDegree);

            RawPoints = new List<Vertex>();
            Vertices = new List<Vertex>();
            Lines = new List<IndexedLine>();

            if (rawPointsList.Count > 0)
            {
                Vertices = rawPointsList;
                Lines = Enumerable.Range(0, rawPointsList.Count - 1)
                    //.Where(t => t % 2 == 0)
                    .Select(t => new IndexedLine(t, t + 1))
                    .ToList();
            }

            if (IsPolygonRenderingEnabled && ControlPoints.Count >= 2)
            {
                var additionalVertices = ControlPoints.Select(t => new Vertex(t, Colors.Gray)).ToList();
                var additionalLines = Enumerable.Range(Vertices.Count, ControlPoints.Count - 1)
                    .Select(t => new IndexedLine(t, t + 1))
                    .ToList();

                Vertices = Vertices.Concat(additionalVertices).ToList();
                Lines = Lines.Concat(additionalLines).ToList();
            }
        }

        public static List<Vertex> SampleBezierCurveC0(IList<Point3D> controlPoints, Func<Point3D, Point3D, double> estimateScreenSpaceDistanceWithoutClip, int targetDegree)
        {
            var rawPointsList = new List<Vertex>();
            for (var i = 0; i + 1 < controlPoints.Count; i += targetDegree)
            {
                var controlPointsLeft = controlPoints.Count - i;
                var degree = Math.Min(controlPointsLeft - 1, targetDegree);

                var bernsteinCoordinates = FillBernsteinCoordinatesArray(controlPoints, degree, i);
                var solver = new DeCastlejauSolver(bernsteinCoordinates);

                var bernsteinPolygonLengthNoClip = EstimateScreenSpacePolygonLength(controlPoints, estimateScreenSpaceDistanceWithoutClip, degree, i);

                var controlPointsWithin = (int) Math.Ceiling(bernsteinPolygonLengthNoClip);
                for (var j = 0; j < controlPointsWithin; ++j)
                {
                    var t = (double) j/controlPointsWithin;
                    var lerped = MathHelpers.MakePoint3D(solver.Evaluate(t));
                    rawPointsList.Add(new Vertex() {Position = lerped, Color = Colors.White});
                }
            }

            return rawPointsList;
        }

        public static double EstimateScreenSpacePolygonLength(IList<Point3D> controlPoints, Func<Point3D, Point3D, double> estimateScreenSpaceDistanceWithoutClip,
            int lineCount, int startIndex)
        {
            var bernsteinPolygonLengthNoClip = 0.0;
            for (var j = 0; j < lineCount; ++j)
            {
                bernsteinPolygonLengthNoClip += estimateScreenSpaceDistanceWithoutClip(
                    controlPoints[startIndex + j],
                    controlPoints[startIndex + j + 1]
                    );
            }
            return bernsteinPolygonLengthNoClip;
        }

        private static double[,] FillBernsteinCoordinatesArray(IList<Point3D> controlPoints, int degree, int i)
        {
            var bernsteinCoordinates = new double[degree + 1, 3];
            for (var j = 0; j < degree + 1; ++j)
            {
                bernsteinCoordinates[j, 0] = controlPoints[i + j].X;
                bernsteinCoordinates[j, 1] = controlPoints[i + j].Y;
                bernsteinCoordinates[j, 2] = controlPoints[i + j].Z;
            }
            return bernsteinCoordinates;
        }
    }
}