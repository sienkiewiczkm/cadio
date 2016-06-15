using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes.Builders;
using CADio.Mathematics.Interfaces;
using CADio.Mathematics.Surfaces;
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

        private readonly BezierSurface _surface = new BezierSurface();

        public void UpdateGeometry(
            Func<Point3D, Point3D, double> estimateScreenDistanceWithoutClip, 
            Predicate<Point3D> isInsideProjectiveCubePredicate
            )
        {
            var isLooped = ControlPoints.Count/(3*SegmentsU + 1) !=
                3*SegmentsV + 1;

            _surface.SegmentsU = SegmentsU;
            _surface.SegmentsV = SegmentsV;
            _surface.ControlPoints = ControlPoints
                .Select(t => t.Position)
                .ToList();

            var builder = new WireframeBuilder();
            var sampler = new SurfaceConstantParameterLinesBuilder(builder);
            sampler.Build(_surface);

            if (IsPolygonRenderingEnabled)
            {
                var netBuilder = new ControlNetBuilder(builder);
                netBuilder.BuildControlNet(
                    ControlPoints,
                    3*SegmentsU + 1,
                    isLooped
                );
            }
            
            Vertices = builder.Vertices.ToList();
            Lines = builder.Lines.ToList();
            MarkerPoints = ControlPoints.Select(t => new Vertex(
                t.Position,
                t.ColorOverride ?? Colors.White
            )).ToList();
            RawPoints = new List<Vertex>();
        }

        public static List<T> GetList2DSubRectCyclic<T>(
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

            for (var j = 0; j < h; ++j)
            {
                for (var i = 0; i < w; ++i)
                {
                    var adjustedY = (y + j)%rows;
                    var adjustedX = (x + i)%rowLength;
                    var index = adjustedY*rowLength + adjustedX;
                    points.Add(input[index]);
                }
            }

            return points;
        }

        public IParametricSurface GetParametricSurface()
        {
            _surface.SegmentsU = SegmentsU;
            _surface.SegmentsV = SegmentsV;
            _surface.ControlPoints = ControlPoints
                .Select(t => t.Position)
                .ToList();
            return _surface;
        }
    }
}