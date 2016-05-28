using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes;
using CADio.Geometry.Shapes.Static;
using CADio.Mathematics;
using CADio.SceneManagement.Interfaces;
using CADio.Views.DragDropSupport;

namespace CADio.SceneManagement
{
    public class WorldObject : 
        IWorldObject, 
        IUIDragable, 
        INotifyPropertyChanged, 
        ISceneSelectable
    {
        private string _name;
        private bool _isSelected;
        private bool _isGrabbed;

        public string Name
        {
            get { return _name ?? Shape?.Name; }
            set { _name = value; OnPropertyChanged(); }
        }

        public Scene SceneManager { get; set; }
        public Point3D Position { get; set; }
        public Point3D WorldPosition => Position;
        public Vector3D Orientation { get; set; } // Euler angles
        public Vector3D Scale { get; set; } = new Vector3D(1, 1, 1);
        public bool IsGrabable { get; set; } = true;
        public IShape Shape { get; set; }

        public IList<IControlPointDependent> ObjectControlPointUsers
            { get; set; } = new List<IControlPointDependent>();

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

        public Color? ColorOverride => _isGrabbed 
            ? Colors.OrangeRed 
            : (Color?) null;

        public Matrix4X4 GetWorldMatrix()
        {
            var translation = Transformations3D.Translation((Vector3D)Position);
            var rotation = Transformations3D.RotationX(Orientation.X)
                           *Transformations3D.RotationY(Orientation.Y)
                           *Transformations3D.RotationZ(Orientation.Z);
            var scaling = Transformations3D.Scaling(Scale);
            return translation*rotation*scaling;
        }

        public virtual void PrerenderUpdate()
        {
        }

        public void DetachFromCompositors()
        {
            foreach (var user in ObjectControlPointUsers)
                user.DetachObjectReferences(this);
            ObjectControlPointUsers.Clear();
        }

        public virtual void Translate(Vector3D translation)
        {
            Position += translation;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual ICollection<ISceneSelectable> GetSelectableChildren()
        {
            return new List<ISceneSelectable>();
        }

        protected virtual void OnPropertyChanged(
            [CallerMemberName] string propertyName = null
            )
        {
            PropertyChanged?.Invoke(
                this, 
                new PropertyChangedEventArgs(propertyName)
            );
        }
    }
}