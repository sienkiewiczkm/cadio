using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using CADio.Mathematics;
using CADio.Mathematics.Numerical;

namespace CADio.Geometry.Shapes.Dynamic
{
    public class BezierPatch : SurfacePatch
    {
        public override string Name => "Bezier Patch";

        protected override void CreateDirectionalSurfaceSampling(
            Func<Point3D, Point3D, double> estimateScreenDistanceWithoutClip, 
            Func<int, int, Tuple<int, int>> mapping, 
            int subdivisions, 
            List<Vertex> vertices, 
            List<IndexedLine> lines
            )
        {
            var solvers = new DeCastlejauSolver[4];
            for (var i = 0; i < 4; ++i)
            {
                var subcontrolPoints = new List<Point3D>();

                for (var j = 0; j < 4; ++j)
                {
                    var mapped = mapping(i, j);
                    subcontrolPoints.Add(ControlPoints[mapped.Item1 + mapped.Item2 * 4].Position);
                }

                solvers[i] = new DeCastlejauSolver(BezierCurveC0.FillBernsteinCoordinatesArray(subcontrolPoints, 3, 0));
            }

            for (var i = 0; i < subdivisions; ++i)
            {
                var t = (double)i / (subdivisions - 1);
                var subdivisionControlPoints = new List<Point3D>();
                for (var j = 0; j < 4; ++j)
                {
                    subdivisionControlPoints.Add(MathHelpers.MakePoint3D(solvers[j].Evaluate(t)));
                }

                var sampled = BezierCurveC0.SampleBezierCurveC0(subdivisionControlPoints,
                    estimateScreenDistanceWithoutClip, 3);
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