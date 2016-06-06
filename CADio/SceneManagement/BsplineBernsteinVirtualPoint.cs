using System.Windows.Media.Media3D;
using CADio.SceneManagement.Curves;
using CADio.SceneManagement.Interfaces;

namespace CADio.SceneManagement
{
    public class BSplineBernsteinVirtualPoint : ISceneSelectable
    {
        private readonly BezierC2WorldObject _bspline;

        public Point3D Position { get; set; }
        public Point3D WorldPosition => Position;
        public bool IsGrabbed { get; set; }

        public BSplineBernsteinVirtualPoint(BezierC2WorldObject bspline)
        {
            _bspline = bspline;
        }

        public void Translate(Vector3D translation)
        {
            _bspline.RequestMovement(this, translation);
        }
    }
}