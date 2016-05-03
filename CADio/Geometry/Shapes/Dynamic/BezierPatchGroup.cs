using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CADio.Geometry.Shapes.Dynamic
{
    public class BezierPatchGroup : IDynamicShape
    {
        public string Name => "Bezier Patch Group";
        public IList<Vertex> Vertices { get; private set; }
        public IList<IndexedLine> Lines { get; private set; }
        public IList<Vertex> MarkerPoints { get; private set; }
        public IList<Vertex> RawPoints { get; private set; }
        public bool IsPolygonRenderingEnabled { get; set; }

        public Control GetEditorControl() => null;

        public int SegmentsX { get; set; }
        public int SegmentsY { get; set; }
        public List<Point3D> ControlPoints { get; set; }
        public int ControlPointsRowLength { get; set; }

        public void UpdateGeometry(Func<Point3D, Point3D, double> estimateScreenSpaceDistanceWithoutClip, Predicate<Point3D> isInsideProjectiveCubePredicate)
        {
            var vertices = new List<Vertex>();
            var lines = new List<IndexedLine>();
            var markerPoints = new List<Vertex>();
            
            for (var i = 0; i < SegmentsX; ++i)
            {
                for (var j = 0; j < SegmentsY; ++j)
                {
                    var patchControlPoints = GetList2DSubRect(ControlPoints, ControlPointsRowLength, 3*i, 3*j, 4, 4);
                    var patch = new BezierPatch
                    {
                        ControlPoints = patchControlPoints,
                        IsPolygonRenderingEnabled = IsPolygonRenderingEnabled
                    };

                    patch.UpdateGeometry(estimateScreenSpaceDistanceWithoutClip, isInsideProjectiveCubePredicate);

                    lines.AddRange(patch.Lines
                        .Select(t => new IndexedLine(t.First + vertices.Count, t.Second + vertices.Count))
                        .ToList()
                    );

                    vertices.AddRange(patch.Vertices);
                    markerPoints.AddRange(patch.MarkerPoints);
                }
            }
            
            Vertices = vertices;
            Lines = lines;
            MarkerPoints = markerPoints;//ControlPoints.Select(t => new Vertex(t, Colors.Red)).ToList();
            RawPoints = new List<Vertex>();
        }

        private List<T> GetList2DSubRect<T>(IReadOnlyList<T> input, int rowLength, int x, int y, int w, int h)
        {
            var points = new List<T>();
            for (var i = 0; i < w; ++i)
                for (var j = 0; j < h; ++j)
                    points.Add(input[((y + j))*rowLength + (x + i)%rowLength]);

            return points;
        }
    }
}