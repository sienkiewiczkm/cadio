using System.Collections.Generic;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes.Dynamic;

namespace CADio.SceneManagement.Curves
{
    public class PolygonCurveWorldObject : 
        WorldObject
    {
        protected PolygonCurve PolygonCurveShape { get; set; }

        public List<Point3D> ControlPoints { get; }

        public PolygonCurveWorldObject(List<Point3D> controlPoints)
        {
            PolygonCurveShape = new PolygonCurve();
            Shape = PolygonCurveShape;

            ControlPoints = controlPoints;
        }

        public override void PrerenderUpdate()
        {
            PolygonCurveShape.ControlPoints = ControlPoints;
            base.PrerenderUpdate();
        }

        public override void Translate(Vector3D translation)
        {
        }
    }
}