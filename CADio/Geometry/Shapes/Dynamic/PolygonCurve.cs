using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes.Builders;

namespace CADio.Geometry.Shapes.Dynamic
{
    public class PolygonCurve : IDynamicShape
    {
        public string Name => "Polygon curve";

        public IList<Vertex> Vertices { get; private set; }
        public IList<IndexedLine> Lines { get; private set; }
        public IList<Vertex> MarkerPoints { get; }
        public IList<Vertex> RawPoints { get; }

        public IList<Point3D> ControlPoints = new List<Point3D>(); 

        public PolygonCurve()
        {
            MarkerPoints = new List<Vertex>();
            RawPoints = new List<Vertex>();
        }

        public Control GetEditorControl() => null;

        public void UpdateGeometry(
            Func<Point3D, Point3D, double> estimateScreenDistanceWithoutClip,
            Predicate<Point3D> isInsideProjectiveCubePredicate
            )
        {
            var builder = new WireframeBuilder();

            foreach (var cp in ControlPoints)
                builder.Connect(cp, Colors.Red);
            builder.FinishChain();

            Vertices = builder.Vertices.ToList();
            Lines = builder.Lines.ToList();
        }
    }
}