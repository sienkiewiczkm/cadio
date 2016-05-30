using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
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

            /*
            gregoryPatch.SetCornerDerivatives(
                0, 
                new Vector3D(0, 1, 0),
                new Vector3D(0, 1, 0)
            );
            */

            MarkerPoints.Clear();
            var usamples = 64;
            var vsamples = 64;
            for (var i = 0; i < usamples; ++i)
            {
                var u = i/(usamples-1.0);
                for (var j = 0; j < vsamples; ++j)
                {
                    var v = j/(vsamples-1.0);
                    MarkerPoints.Add(new Vertex(gregoryPatch.Evaluate(u, v)));
                }
            }
        }
    }
}