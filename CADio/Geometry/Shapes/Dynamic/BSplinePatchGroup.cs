using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.Configuration;
using CADio.Mathematics.Numerical;
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

        public int SegmentsX { get; set; }
        public int SegmentsY { get; set; }
        public List<SurfaceControlPoint> ControlPoints { get; set; }
        public int ControlPointsRowLength { get; set; }

        public void UpdateGeometry(
            Func<Point3D, Point3D, double> estimateScreenDistanceWithoutClip, 
            Predicate<Point3D> isInsideProjectiveCubePredicate
            )
        {
            var vertices = new List<Vertex>();
            var lines = new List<IndexedLine>();

            Func<int, int, int> idMapping = (i, j) => 
                (j)*(ControlPointsRowLength) + (i%(ControlPointsRowLength)); 
                
            CreateDirectionalSurfaceSampling(
                estimateScreenDistanceWithoutClip,
                (i, j) => new Tuple<int, int>(i, j),
                idMapping,
                3 + SegmentsX, 
                3 + SegmentsY, 
                GlobalSettings.QualitySettingsViewModel.SurfaceHSubdivisions,
                vertices, lines
            );

            CreateDirectionalSurfaceSampling(
                estimateScreenDistanceWithoutClip,
                (i, j) => new Tuple<int, int>(j, i),
                idMapping,
                3 + SegmentsY,
                3 + SegmentsX,
                GlobalSettings.QualitySettingsViewModel.SurfaceWSubdivisions,
                vertices, lines
            );

            if (IsPolygonRenderingEnabled)
            {
                var additionalVertices = ControlPoints
                        .Select(t => new Vertex(
                            t.Position, 
                            Colors.GreenYellow
                        )).ToList();

                var additionalLines = new List<IndexedLine>();
                var baseVertex = vertices.Count;

                for (var x = 0; x < 3 + SegmentsX; ++x)
                {
                    for (var y = 0; y < 3 + SegmentsY; ++y)
                    {
                        if (x < 2 + SegmentsX)
                        {
                            additionalLines.Add(new IndexedLine(
                                baseVertex + (ControlPointsRowLength*y 
                                    + (x%ControlPointsRowLength)),
                                baseVertex + (ControlPointsRowLength*y 
                                    + ((x + 1)%ControlPointsRowLength))
                            ));
                        }
                        if (y < 2 + SegmentsY)
                        {
                            additionalLines.Add(new IndexedLine(
                                baseVertex + (ControlPointsRowLength * y 
                                    + (x % ControlPointsRowLength)),
                                baseVertex + (ControlPointsRowLength * (y + 1) 
                                    + (x % ControlPointsRowLength))
                            ));
                        }
                    }
                }

                vertices = vertices.Concat(additionalVertices).ToList();
                lines = lines.Concat(additionalLines).ToList();
            }

            Vertices = vertices;
            Lines = lines;
            MarkerPoints = ControlPoints
                .Select(t => new Vertex(
                    t.Position, 
                    t.ColorOverride ?? Colors.White
                )).ToList();

            RawPoints = new List<Vertex>();
        }

        protected void CreateDirectionalSurfaceSampling(
            Func<Point3D, Point3D, double> estimateScreenDistanceWithoutClip,
            Func<int, int, Tuple<int, int>> mapping, 
            Func<int, int, int> indexMap,  
            int rows, 
            int cols, 
            int subdivisions, 
            List<Vertex> vertices, 
            List<IndexedLine> lines
            )
        {
            var solvers = new DeBoorSolverRecursive3D[rows];
            for (var i = 0; i < rows; ++i)
            {
                var subcontrolPoints = new List<Point3D>();

                for (var j = 0; j < cols; ++j)
                {
                    var mapped = mapping(i, j);
                    var indexMapped = indexMap(mapped.Item1, mapped.Item2);
                    subcontrolPoints.Add(
                        ControlPoints[indexMapped].Position
                    );
                }

                solvers[i] = new DeBoorSolverRecursive3D(subcontrolPoints);
            }

            for (var i = 0; i < subdivisions; ++i)
            {
                var t = (double)i / (subdivisions - 1);
                var subdivisionControlPoints = new List<Point3D>();
                for (var j = 0; j < rows; ++j)
                {
                    subdivisionControlPoints.Add(solvers[j].Evaluate(t, true));
                }

                var sampled = BezierCurveC2.SampleBSplineCurve(
                    subdivisionControlPoints, 
                    null, 
                    true,
                    estimateScreenDistanceWithoutClip
                );

                if (sampled.Count <= 1) continue;

                var sampledLines = Enumerable
                    .Range(vertices.Count, sampled.Count - 1)
                    .Select(idx => new IndexedLine(idx, idx + 1))
                    .ToList();

                vertices.AddRange(sampled);
                lines.AddRange(sampledLines);
            }
        }
    }
}