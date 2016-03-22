using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes;
using CADio.Mathematics;

namespace CADio.SceneManagement
{
    public class WorldObject
    {
        public string Name { get; set; }
        public Point3D Position { get; set; }
        public Vector3D Orientation { get; set; } // Euler angles
        public Vector3D Scale { get; set; } = new Vector3D(1, 1, 1);
        public bool IsGrabable { get; set; }
        public IShape Shape { get; set; }

        public Matrix4X4 GetWorldMatrix()
        {
            var translation = Transformations3D.Translation((Vector3D) Position);
            var rotation = Transformations3D.RotationX(Orientation.X)
                           *Transformations3D.RotationY(Orientation.Y)
                           *Transformations3D.RotationZ(Orientation.Z);
            var scaling = Transformations3D.Scaling(Scale);
            return translation*rotation*scaling;
        }
    }
}