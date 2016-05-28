using System.Windows.Media.Media3D;
using CADio.SceneManagement.Interfaces;

namespace CADio.SceneManagement
{
    public class VirtualPoint : ISceneSelectable, ICollapsable
    {
        private Point3D _position;

        public Point3D Position
        {
            get
            {
                return Tracked?.Position ?? _position;
            }
            set
            {
                if (Tracked == null) _position = value;
                else Tracked.Position = _position;
            }
        }

        public ICollapsable Tracked { get; set; }
        public bool IsGrabbed { get; set; }

        public void Translate(Vector3D translation)
        {
            Position += translation;
        }
    }
}