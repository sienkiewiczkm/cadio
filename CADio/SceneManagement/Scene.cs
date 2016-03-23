using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CADio.Geometry.Shapes;
using CADio.Mathematics;
using CADio.Rendering;

namespace CADio.SceneManagement
{
    public class Scene : INotifyPropertyChanged
    {
        private WorldObject _grabbedObject;
        private WorldObject _manipulator;
        private ObservableCollection<WorldObject> _objects = new ObservableCollection<WorldObject>();
        private Camera _camera = new Camera();

        public Camera Camera
        {
            get { return _camera; }
            set { _camera = value; OnPropertyChanged(); }
        }

        public ObservableCollection<WorldObject> Objects
        {
            get { return _objects; }
            set { _objects = value; OnPropertyChanged(); }
        }

        public WorldObject Manipulator
        {
            get { return _manipulator; }
            set { _manipulator = value; OnPropertyChanged(); }
        }

        public WorldObject GrabbedObject
        {
            get { return _grabbedObject; }
            set { _grabbedObject = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void AttachObject(WorldObject worldObject)
        {
            if (worldObject == null) return;
            worldObject.Owner?.DetachObject(worldObject);
            worldObject.Owner = this;
            Objects.Add(worldObject);
        }

        public void DetachObject(WorldObject worldObject)
        {
            if (worldObject?.Owner != this) return;
            if (GrabbedObject == worldObject)
                GrabbedObject = null;
            Objects.Remove(worldObject);
            worldObject.Owner = this;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}