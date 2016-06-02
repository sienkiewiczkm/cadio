using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CADio.Geometry.Shapes.Dynamic;

namespace CADio.SceneManagement.Surfaces
{
    class TriangularHoleFill : WorldObject
    {
        public TriangularHoleFillShape TriangularHoleFillShape;
        public IList<VirtualPoint> OutlinePoints;

        public TriangularHoleFill()
        {
            TriangularHoleFillShape = new TriangularHoleFillShape();
            Shape = TriangularHoleFillShape;
        }

        public override void PrerenderUpdate()
        {
            base.PrerenderUpdate();
            TriangularHoleFillShape.ReferencePoints = OutlinePoints;

            // todo: remove hack!
            TriangularHoleFillShape.ReferenceSurfaces = 
                new List<BezierSurfaceWorldObject>()
            {
                OutlinePoints[1].ParentObject as BezierSurfaceWorldObject,
                OutlinePoints[4].ParentObject as BezierSurfaceWorldObject,
                OutlinePoints[7].ParentObject as BezierSurfaceWorldObject
            };
        }
    }
}
