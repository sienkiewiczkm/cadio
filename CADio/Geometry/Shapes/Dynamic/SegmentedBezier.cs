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
    public class SegmentedBezier : IDynamicShape
    {
        public string Name => "Segmented Bezier";

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

            var rawPointsList = new List<Vertex>();
            for (var i = 0; i + 1 < ControlPoints.Count; i += targetDegree)
            {
                var controlPointsLeft = ControlPoints.Count - i;
                var degree = Math.Min(controlPointsLeft - 1, targetDegree);

                var bernsteinCoordinates = FillBernsteinCoordinatesArray(degree, i);
                var solver = new DeCastlejauSolver(bernsteinCoordinates);

                var bernsteinPolygonLengthNoClip = 0.0;
                for (var j = 0; j < degree; ++j)
                {
                    bernsteinPolygonLengthNoClip += estimateScreenSpaceDistanceWithoutClip(
                        ControlPoints[i + j], 
                        ControlPoints[i + j + 1]
                    );
                }

                var controlPointsWithin = (int)Math.Ceiling(bernsteinPolygonLengthNoClip);
                for (var j = 0; j < controlPointsWithin; ++j)
                {
                    var t = (double)j/controlPointsWithin;
                    var lerped = MathHelpers.MakePoint3D(solver.Evaluate(t));
                    rawPointsList.Add(new Vertex() {Position = lerped, Color = Colors.White});
                }
            }

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

        private double[,] FillBernsteinCoordinatesArray(int degree, int i)
        {
            var bernsteinCoordinates = new double[degree + 1, 3];
            for (var j = 0; j < degree + 1; ++j)
            {
                bernsteinCoordinates[j, 0] = ControlPoints[i + j].X;
                bernsteinCoordinates[j, 1] = ControlPoints[i + j].Y;
                bernsteinCoordinates[j, 2] = ControlPoints[i + j].Z;
            }
            return bernsteinCoordinates;
        }
    }
}