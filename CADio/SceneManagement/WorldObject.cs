using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes;
using CADio.Mathematics;

namespace CADio.SceneManagement
{
    public class WorldObject
    {
        private string _name;

        public string Name
        {
            get { return _name ?? Shape?.Name; }
            set { _name = value; }
        }

        public Scene Owner { get; set; }
        public Point3D Position { get; set; }
        public Vector3D Orientation { get; set; } // Euler angles
        public Vector3D Scale { get; set; } = new Vector3D(1, 1, 1);
        public bool IsGrabable { get; set; } = true;
        public IShape Shape { get; set; }

        public bool IsGrabbed => Owner?.GrabbedObject == this;

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