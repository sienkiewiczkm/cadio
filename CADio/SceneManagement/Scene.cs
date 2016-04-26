using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using CADio.Geometry.Shapes;
using CADio.Mathematics;
using CADio.Rendering;

namespace CADio.SceneManagement
{
    public class Scene : INotifyPropertyChanged
    {
        private ISceneSelectable _grabbedObject;
        private WorldObject _manipulator;
        private ObservableCollection<WorldObject> _objects;
        private ICamera _camera = new ArcBallCamera();
        private ICollectionView _manageableObjects;
        private WorldObject _firstSelectedObject;
        private ISmartEditTarget _smartEditTarget;

        public ICamera Camera
        {
            get { return _camera; }
            set { _camera = value; OnPropertyChanged(); }
        }

        public ISmartEditTarget SmartEditTarget
        {
            get { return _smartEditTarget; }
            set
            {
                var oldEditTarget = _smartEditTarget;
                _smartEditTarget = value;

                oldEditTarget?.NotifyAboutStateChange();
                _smartEditTarget?.NotifyAboutStateChange();

                OnPropertyChanged();
            }
        }

        public ObservableCollection<WorldObject> Objects
        {
            get { return _objects; }
            set
            {
                _objects = value;
                OnPropertyChanged();
                CreateObjectCollectionView();
            }
        }

        private void CreateObjectCollectionView()
        {
            var collectionView = CollectionViewSource.GetDefaultView(Objects);
            collectionView.Filter = o => ((WorldObject) o).IsGrabable;
            ManageableObjects = collectionView;
        }

        public ICollectionView ManageableObjects
        {
            get { return _manageableObjects; }
            protected set { _manageableObjects = value; OnPropertyChanged(); }
        }

        public WorldObject Manipulator
        {
            get { return _manipulator; }
            set { _manipulator = value; OnPropertyChanged(); }
        }

        public ISceneSelectable GrabbedObject
        {
            get { return _grabbedObject; }
            set
            {
                if (_grabbedObject != null)
                    _grabbedObject.IsGrabbed = false;
                _grabbedObject = value;
                if (_grabbedObject != null)
                    _grabbedObject.IsGrabbed = true;
                OnPropertyChanged();
            }
        }

        public WorldObject FirstSelectedObject
        {
            get { return _firstSelectedObject; }
            set { _firstSelectedObject = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Scene()
        {
            Objects = new ObservableCollection<WorldObject>();
        }

        public void AttachObject(WorldObject worldObject)
        {
            if (worldObject == null) return;
            worldObject.SceneManager?.DetachObject(worldObject);
            worldObject.SceneManager = this;
            worldObject.PropertyChanged += OnWorldObjectChange;
            Objects.Add(worldObject);
        }

        public void DetachObject(IWorldObject worldObject)
        {
            if (worldObject?.SceneManager != this) return;
            if (GrabbedObject == worldObject)
                GrabbedObject = null;
            worldObject.DetachFromCompositors();
            // todo: fix hack
            Objects.Remove((WorldObject)worldObject);
            worldObject.SceneManager = null;
            worldObject.PropertyChanged -= OnWorldObjectChange;
        }

        private void OnWorldObjectChange(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(WorldObject.IsSelected):
                    // todo: fix this workaround, need to pass OnPropertyChanged to parent somehow
                    FirstSelectedObject = Objects.FirstOrDefault(t => t.IsSelected);
                    break;
                default:
                    break;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}