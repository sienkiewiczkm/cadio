using System;
using System.Collections.Generic;
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

        public IList<Vertex> Vertices => new List<Vertex>();
        public IList<IndexedLine> Lines => new List<IndexedLine>();
        public IList<Vertex> MarkerPoints => new List<Vertex>();
        public IList<Vertex> RawPoints { get; private set; }

        public Control GetEditorControl() => null;

        public IList<Point3D> ControlPoints; 

        public void UpdateGeometry()
        {
            const int targetDegree = 1;

            var rawPointsList = new List<Vertex>();
            for (var i = 0; i + 1 < ControlPoints.Count; i += targetDegree)
            {
                var controlPointsLeft = ControlPoints.Count - i;
                var degree = Math.Min(controlPointsLeft - 1, targetDegree);

                var bernsteinCoordinates = new double[degree + 1, 3];
                for (var j = 0; j < degree + 1; ++j)
                {
                    bernsteinCoordinates[j, 0] = ControlPoints[i+j].X;
                    bernsteinCoordinates[j, 1] = ControlPoints[i+j].Y;
                    bernsteinCoordinates[j, 2] = ControlPoints[i+j].Z;
                }

                var solver = new DeCastlejauSolver(bernsteinCoordinates);

                // Generate raw points for segment
                var controlPointsWithin = 1000 + 1; // todo: dynamic
                for (var j = 0; j < controlPointsWithin; ++j)
                {
                    var t = (double)j/controlPointsWithin;
                    var lerped = MathHelpers.MakePoint3D(solver.Evaluate(t));
                    rawPointsList.Add(new Vertex() {Position = lerped, Color = Colors.White});
                }
            }

            RawPoints = rawPointsList;
        }
    }
}