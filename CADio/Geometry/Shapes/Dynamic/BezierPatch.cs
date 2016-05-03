using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.Mathematics;
using CADio.Mathematics.Numerical;

namespace CADio.Geometry.Shapes.Dynamic
{
    public class BezierPatch : IDynamicShape
    {
        public string Name => "Bezier Patch";

        public IList<Vertex> Vertices { get; private set; }
        public IList<IndexedLine> Lines { get; private set; }
        public IList<Vertex> MarkerPoints { get; private set; }
        public IList<Vertex> RawPoints { get; private set; }
        public bool IsPolygonRenderingEnabled { get; set; }

        public Control GetEditorControl() => null;

        /// <summary>
        /// 16 Control Points
        /// </summary>
        public List<Point3D> ControlPoints { get; set; }

        public void UpdateGeometry(Func<Point3D, Point3D, double> estimateScreenSpaceDistanceWithoutClip, Predicate<Point3D> isInsideProjectiveCubePredicate)
        {
            RawPoints = new List<Vertex>();
            MarkerPoints = new List<Vertex>();
            Vertices = new List<Vertex>();
            Lines = new List<IndexedLine>();
            
            if (ControlPoints.Count != 16)
                return;

            // generate subdivisions
            var vertices = new List<Vertex>();
            var lines = new List<IndexedLine>();

            Func<int, int, Tuple<int,int>> mapping = (i, j) => new Tuple<int, int>(i, j);
            var subdivisions = 8;

            CreateDirectionalSurfaceSampling(
                estimateScreenSpaceDistanceWithoutClip, 
                (i, j) => new Tuple<int, int>(i, j), 
                subdivisions, vertices, lines
            );

            CreateDirectionalSurfaceSampling(
                estimateScreenSpaceDistanceWithoutClip,
                (i, j) => new Tuple<int, int>(j, i),
                subdivisions, vertices, lines
            );

            var additionalVertices = ControlPoints.Select(t => new Vertex(t, Colors.Gray)).ToList();
            var additionalLines = new List<IndexedLine>();
            var baseVertex = vertices.Count;

            for (var x = 0; x < 4; ++x)
            {
                for (var y = 0; y < 4; ++y)
                {
                    if (x < 3)
                    {
                        additionalLines.Add(new IndexedLine(baseVertex + (4*y + x), baseVertex + (4*y + x + 1)));
                    }
                    if (y < 3)
                    {
                        additionalLines.Add(new IndexedLine(baseVertex + (4*y + x), baseVertex + (4*(y + 1) + x)));
                    }
                }
            }

            vertices.AddRange(additionalVertices);
            lines.AddRange(additionalLines);

            MarkerPoints = ControlPoints.Select(t => new Vertex(t, Colors.White)).ToList();
            Vertices = vertices;//Vertices.Concat(additionalVertices).ToList();
            Lines = lines; //Lines.Concat(additionalLines).ToList();
        }

        private void CreateDirectionalSurfaceSampling(Func<Point3D, Point3D, double> estimateScreenSpaceDistanceWithoutClip, Func<int, int, Tuple<int, int>> mapping,
            int subdivisions, List<Vertex> vertices, List<IndexedLine> lines)
        {
            var solvers = new DeCastlejauSolver[4];
            for (var i = 0; i < 4; ++i)
            {
                var subcontrolPoints = new List<Point3D>();

                for (var j = 0; j < 4; ++j)
                {
                    var mapped = mapping(i, j);
                    subcontrolPoints.Add(ControlPoints[mapped.Item1 + mapped.Item2*4]);
                }

                solvers[i] = new DeCastlejauSolver(BezierCurveC0.FillBernsteinCoordinatesArray(subcontrolPoints, 3, 0));
            }

            for (var i = 0; i < subdivisions; ++i)
            {
                var t = (double) i/(subdivisions - 1);
                var subdivisionControlPoints = new List<Point3D>();
                for (var j = 0; j < 4; ++j)
                {
                    subdivisionControlPoints.Add(MathHelpers.MakePoint3D(solvers[j].Evaluate(t)));
                }

                var sampled = BezierCurveC0.SampleBezierCurveC0(subdivisionControlPoints,
                    estimateScreenSpaceDistanceWithoutClip, 3);
                if (sampled.Count <= 1) continue;

                var sampledLines = Enumerable.Range(vertices.Count, sampled.Count - 1)
                    .Select(idx => new IndexedLine(idx, idx + 1))
                    .ToList();

                vertices.AddRange(sampled);
                lines.AddRange(sampledLines);
            }
        }
    }
}