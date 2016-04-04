using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.Mathematics;

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

                // Generate raw points for segment
                var controlPointsWithin = 1000 + 1; // todo: dynamic
                for (var j = 0; j < controlPointsWithin; ++j)
                {
                    var t = (double)j/controlPointsWithin;
                    var lerped = MathHelpers.Lerp(ControlPoints[i], ControlPoints[i + 1], t);
                    rawPointsList.Add(new Vertex() {Position = lerped, Color = Colors.White});
                }
            }

            RawPoints = rawPointsList;
        }
    }
}