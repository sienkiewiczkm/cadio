using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes;
using CADio.Mathematics;

namespace CADio.SceneManagement
{
    public class WorldObject : INotifyPropertyChanged
    {
        private string _name;
        private bool _isSelected;
        private bool _isGrabbed;

        public string Name
        {
            get { return _name ?? Shape?.Name; }
            set { _name = value; OnPropertyChanged(); }
        }

        public Scene Owner { get; set; }
        public Point3D Position { get; set; }
        public Vector3D Orientation { get; set; } // Euler angles
        public Vector3D Scale { get; set; } = new Vector3D(1, 1, 1);
        public bool IsGrabable { get; set; } = true;
        public IShape Shape { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public bool IsGrabbed
        {
            get { return _isGrabbed; }
            set { _isGrabbed = value; OnPropertyChanged(); }
        }

        public Matrix4X4 GetWorldMatrix()
        {
            var translation = Transformations3D.Translation((Vector3D) Position);
            var rotation = Transformations3D.RotationX(Orientation.X)
                           *Transformations3D.RotationY(Orientation.Y)
                           *Transformations3D.RotationZ(Orientation.Z);
            var scaling = Transformations3D.Scaling(Scale);
            return translation*rotation*scaling;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}