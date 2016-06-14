using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.Configuration;
using CADio.Geometry.Shapes.Builders;
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

        public void UpdateGeometry(
            Func<Point3D, Point3D, double> estimateScreenDistanceWithoutClip, 
            Predicate<Point3D> isInsideProjectiveCubePredicate
            )
        {
            var vertices = new List<Vertex>();
            var lines = new List<IndexedLine>();

            var dataRowLength = 3 + SegmentsU;
            var dataRowsCount = ControlPoints.Count/dataRowLength;

            var builder = new WireframeBuilder();
            var sampler = new SurfaceConstantParameterLinesBuilder(builder);

            var bsplineSurf = new BsplineSurface()
            {
                ControlPoints = ControlPoints.Select(t => t.Position).ToList(),
                SegmentsU = SegmentsU,
                SegmentsV = SegmentsV,
            };

            sampler.Build(bsplineSurf);

            if (IsPolygonRenderingEnabled)
            {
                var polygonColor = Colors.GreenYellow;
                for (var x = 0; x < 3 + SegmentsU; ++x)
                {
                    for (var y = 0; y < 3 + SegmentsV; ++y)
                    {
                        var ym = y%dataRowsCount;
                        var fromPoint = 
                            ControlPoints[dataRowLength*ym + x].Position;

                        if (x < 2 + SegmentsU)
                        {
                            var toPointIndex = dataRowLength*ym + x + 1;
                            builder.Connect(fromPoint, polygonColor);
                            builder.Connect(
                                ControlPoints[toPointIndex].Position,
                                polygonColor
                            );
                            builder.FinishChain();
                        }

                        if (y < 2 + SegmentsV)
                        {
                            var toPointIndex =
                                dataRowLength*((ym + 1)%dataRowsCount) + x;
                            builder.Connect(fromPoint, polygonColor);
                            builder.Connect(
                                ControlPoints[toPointIndex].Position,
                                polygonColor
                            );
                            builder.FinishChain();
                        }
                    }
                }
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
    }
}