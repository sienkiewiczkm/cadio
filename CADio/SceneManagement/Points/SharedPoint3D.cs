using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace CADio.SceneManagement.Points
{
    public class SharedPoint3D
    {
        public Point3D Data { get; set; }
        public List<VirtualPoint> Users { get; set; } = 
            new List<VirtualPoint>();

        public SharedPoint3D()
        {
        }

        public SharedPoint3D(VirtualPoint originalOwner)
        {
            Users.Add(originalOwner);
        }
    }
}