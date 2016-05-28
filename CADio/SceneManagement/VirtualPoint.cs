using System.Collections.Generic;
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

        public Point3D WorldPosition
        {
            get
            {
                if (ParentObject != null)
                    return ParentObject.Position + (Vector3D)Position;
                return Position;
            }
        }

        public ICollapsable Tracked { get; set; }

        public List<ICollapsable> Trackers { get; } = new List<ICollapsable>();

        public bool IsGrabbed { get; set; }
        public IWorldObject ParentObject { get; set; }

        public void Translate(Vector3D translation)
        {
            Position += translation;
        }
    }
}