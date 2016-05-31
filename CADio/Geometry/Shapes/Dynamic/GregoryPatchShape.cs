using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes.Builders;
using CADio.Mathematics.Patches;
using CADio.SceneManagement.Surfaces;

namespace CADio.Geometry.Shapes.Dynamic
{
    public class GregoryPatchShape : IDynamicShape
    {
        public string Name { get; }
        public IList<Vertex> Vertices { get; set; } = new List<Vertex>();
        public IList<IndexedLine> Lines { get; set; } = new List<IndexedLine>();
        public IList<Vertex> MarkerPoints { get; set; } = new List<Vertex>();
        public IList<Vertex> RawPoints { get; set; } = new List<Vertex>();
        public Control GetEditorControl() => null;

        public void UpdateGeometry(
            Func<Point3D, Point3D, double> estimateScreenDistanceWithoutClip, 
            Predicate<Point3D> isInsideProjectiveCubePredicate
            )
        {
            var gregoryPatch = new GregoryPatch();
            gregoryPatch.SetCornerPoint(0, new Point3D(-1.0f, 0.0f, +1.0f));
            gregoryPatch.SetCornerPoint(1, new Point3D(+1.0f, 0.0f, +1.0f));
            gregoryPatch.SetCornerPoint(2, new Point3D(-1.0f, 0.0f, -1.0f));
            gregoryPatch.SetCornerPoint(3, new Point3D(+1.0f, 0.0f, -1.0f));

            gregoryPatch.SetCornerDerivatives(
                0, 
                new Vector3D(0, 1, 0),
                new Vector3D(0, 1, 0)
            );

            var builder = new WireframeBuilder();
            var surfaceSampler = new SurfaceConstantParameterLinesBuilder(
                builder
            );

            surfaceSampler.Build(gregoryPatch);

            for (var i = 0; i < 4; ++i)
            {
                var cornerPos = gregoryPatch.GetCornerPoint(i);
                var cornerdU = gregoryPatch.GetCornerDerivativeU(i);
                var cornerdV = gregoryPatch.GetCornerDerivativeV(i);
                builder.DrawVector(cornerPos, cornerdU);
                builder.DrawVector(cornerPos, cornerdV);
            }

            Vertices = builder.Vertices.ToList();
            Lines = builder.Lines.ToList();
        }
    }
}