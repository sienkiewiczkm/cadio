using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Media3D;
using CADio.SceneManagement.Interfaces;

namespace CADio.SceneManagement.Points
{
    public class VirtualPoint : ISceneSelectable 
    {
        private SharedPoint3D _sharedPoint;

        public Point3D Position
        {
            get { return _sharedPoint.Data; }
            set { _sharedPoint.Data = value; }
        }

        public Point3D WorldPosition => Position;

        public bool IsGrabbed { get; set; }
        public IWorldObject ParentObject { get; set; }

        public SharedPoint3D SharedPoint
        {
            get { return _sharedPoint; }
            set
            {
                Debug.Assert(_sharedPoint != null);
                _sharedPoint.Users.Remove(this);
                _sharedPoint = value;
                _sharedPoint.Users.Add(this);
            }
        }

        public VirtualPoint()
        {
            _sharedPoint = new SharedPoint3D(this);
        }

        public void Translate(Vector3D translation)
        {
            Position += translation;
        }

        public void MergeInto(VirtualPoint another)
        {
            var sharedPointUsersCopy = _sharedPoint.Users.ToList();
            foreach (var formerUser in sharedPointUsersCopy)
            {
                formerUser._sharedPoint.Users.Remove(formerUser);
                formerUser._sharedPoint = another._sharedPoint;
                another._sharedPoint.Users.Add(formerUser);
            }
        }

        public void SeparateSingle()
        {
            _sharedPoint?.Users?.Remove(this);
            _sharedPoint = new SharedPoint3D(this);
        }

        public List<IWorldObject> GetAdjacentObjects()
        {
            return _sharedPoint.Users.Select(t => t.ParentObject).ToList();
        }

        public VirtualPoint GetAdjacentVirtualPointFor(IWorldObject adjacent)
        {
            return _sharedPoint.Users.FirstOrDefault(
                t => t.ParentObject == adjacent
            );
        }
    }
}