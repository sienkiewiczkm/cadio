using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes;
using CADio.Helpers.MVVM;
using CADio.SceneManagement;

namespace CADio.ViewModels
{
    internal class SceneTreeViewModel : INotifyPropertyChanged
    {
        private static int _sessionPointId = 1;

        private Scene _scene;
        private WorldObject _selectedObject;
        private ICommand _createPointCommand;
        private ICommand _removeSelectedObjectCommand;
        private ICollectionView _listableObjects;

        public event PropertyChangedEventHandler PropertyChanged;

        public Scene Scene
        {
            get { return _scene; }
            set
            {
                if (_scene != null)
                    _scene.PropertyChanged -= ScenePropertyChanged;
                _scene = value;
                _scene.PropertyChanged += ScenePropertyChanged;

                CreateSourceView();
                OnPropertyChanged();
            }
        }

        private void ScenePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // todo: remove workaround :(
            CreateSourceView();
        }

        private void CreateSourceView()
        {
            var view = CollectionViewSource.GetDefaultView(Scene.Objects);
            view.Filter = obj => ((WorldObject) obj).IsGrabable;
            ListableObjects = view;
        }

        public ICollectionView ListableObjects
        {
            get { return _listableObjects; }
            set { _listableObjects = value; OnPropertyChanged(); }
        }

        public WorldObject SelectedObject
        {
            get { return _selectedObject; }
            set
            {
                _selectedObject = value;
                OnPropertyChanged();
            }
        }

        public ICommand CreatePointCommand
        {
            get { return _createPointCommand ?? (_createPointCommand = new RelayCommand(CreatePoint)); }
            set { _createPointCommand = value; OnPropertyChanged(); }
        }

        public ICommand RemoveSelectedObjectCommand
        {
            get
            {
                return _removeSelectedObjectCommand ?? 
                    (_removeSelectedObjectCommand = new RelayCommand(RemoveSelectedObject, () => SelectedObject != null));
            }
            set { _removeSelectedObjectCommand = value; }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CreatePoint()
        {
            var newPoint = new WorldObject()
            {
                Name = "Point " + _sessionPointId++,
                Position = GetInsertionLocation(),
                Shape = new MarkerPoint(),
            };

            _scene.AttachObject(newPoint);
        }

        private void RemoveSelectedObject()
        {
            var objectToRemove = SelectedObject;
            SelectedObject = null;
            _scene.DetachObject(objectToRemove);
        }

        private Point3D GetInsertionLocation()
        {
            return _scene?.Manipulator?.Position ?? new Point3D(0, 0, 0);
        }
    }
}