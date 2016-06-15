using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.Configuration;
using CADio.Geometry.Shapes.Builders;
using CADio.Mathematics.Interfaces;
using CADio.Mathematics.Numerical;
using CADio.Mathematics.Surfaces;
using CADio.SceneManagement.Surfaces;

namespace CADio.Geometry.Shapes.Dynamic
{
    public class BSplinePatchGroup : IDynamicShape
    {
        public string Name => "BSpline Patch Group";
        public IList<Vertex> Vertices { get; private set; }
        public IList<IndexedLine> Lines { get; private set; }
        public IList<Vertex> MarkerPoints { get; private set; }
        public IList<Vertex> RawPoints { get; private set; }
        public bool IsPolygonRenderingEnabled { get; set; }

        public Control GetEditorControl() => null;

        public int SegmentsU { get; set; }
        public int SegmentsV { get; set; }
        public List<SurfaceControlPoint> ControlPoints { get; set; }
        private readonly BsplineSurface _surface = new BsplineSurface();

        public void UpdateGeometry(
            Func<Point3D, Point3D, double> estimateScreenDistanceWithoutClip, 
            Predicate<Point3D> isInsideProjectiveCubePredicate
            )
        {
            var dataRowLength = 3 + SegmentsU;
            var dataRowsCount = ControlPoints.Count/dataRowLength;

            var isLooped = dataRowsCount != 3 + SegmentsV;

            var builder = new WireframeBuilder();
            var sampler = new SurfaceConstantParameterLinesBuilder(builder);

            var bsplineSurf = GetParametricSurface();

            sampler.Build(bsplineSurf);

            if (IsPolygonRenderingEnabled)
            {
                var netBuilder = new ControlNetBuilder(builder);
                netBuilder.BuildControlNet(
                    ControlPoints,
                    3 + SegmentsU,
                    isLooped
                );
            }

            Vertices = builder.Vertices.ToList();
            Lines = builder.Lines.ToList();
            MarkerPoints = ControlPoints
                .Select(t => new Vertex(
                    t.Position, 
                    t.ColorOverride ?? Colors.White
                )).ToList();

            RawPoints = new List<Vertex>();
        }

        public IParametricSurface GetParametricSurface()
        {
            _surface.ControlPoints =
                ControlPoints.Select(t => t.Position).ToList();
            _surface.SegmentsU = SegmentsU;
            _surface.SegmentsV = SegmentsV;
            return _surface;
        }
    }
}