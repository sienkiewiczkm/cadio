using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes.Builders;
using CADio.Mathematics;
using CADio.Mathematics.Numerical;
using CADio.Mathematics.Patches;

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
            if (vertices.Count > 0)
                return;

            var builder = new WireframeBuilder();
            var surfaceSampler = new SurfaceConstantParameterLinesBuilder(
                builder
            );

            var bezierPatch = new BernsteinPatch();
            for (var i = 0; i < 16; ++i)
            {
                bezierPatch.ControlPoints[i/4, i%4] = ControlPoints[i].Position;
            }
            surfaceSampler.Build(bezierPatch);

            vertices.AddRange(builder.Vertices.ToList());
            lines.AddRange(builder.Lines.ToList());
        }
    }
}