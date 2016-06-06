using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.SceneManagement.Surfaces;

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

        public int SegmentsU { get; set; }
        public int SegmentsV { get; set; }
        public List<SurfaceControlPoint> ControlPoints { get; set; }

        public void UpdateGeometry(
            Func<Point3D, Point3D, double> estimateScreenDistanceWithoutClip, 
            Predicate<Point3D> isInsideProjectiveCubePredicate
            )
        {
            var vertices = new List<Vertex>();
            var lines = new List<IndexedLine>();
            var markerPoints = new List<Vertex>();
            
            for (var i = 0; i < SegmentsU; ++i)
            {
                for (var j = 0; j < SegmentsV; ++j)
                {
                    var patchControlPoints = GetList2DSubRectCyclic(
                        ControlPoints, 
                        3*SegmentsU+1, 
                        3*i, 
                        3*j, 
                        4, 
                        4
                    );
                    
                    var patch = new BezierPatch
                    {
                        ControlPoints = patchControlPoints,
                        IsPolygonRenderingEnabled = IsPolygonRenderingEnabled
                    };

                    patch.UpdateGeometry(
                        estimateScreenDistanceWithoutClip, 
                        isInsideProjectiveCubePredicate
                    );

                    lines.AddRange(patch.Lines
                        .Select(t => new IndexedLine(
                            t.First + vertices.Count, 
                            t.Second + vertices.Count
                        ))
                        .ToList()
                    );

                    vertices.AddRange(patch.Vertices);
                    markerPoints.AddRange(patch.MarkerPoints);
                }
            }
            
            Vertices = vertices;
            Lines = lines;
            MarkerPoints = markerPoints;
            RawPoints = new List<Vertex>();
        }

        private static List<T> GetList2DSubRectCyclic<T>(
            IReadOnlyList<T> input, 
            int rowLength, 
            int x, 
            int y, 
            int w, 
            int h
            )
        {
            var points = new List<T>();
            var rows = input.Count/rowLength;

            for (var i = 0; i < w; ++i)
            {
                for (var j = 0; j < h; ++j)
                {
                    var adjustedY = (y + j)%rows;
                    var adjustedX = (x + i)%rowLength;
                    var index = adjustedY*rowLength + adjustedX;
                    points.Add(input[index]);
                }
            }

            return points;
        }
    }
}