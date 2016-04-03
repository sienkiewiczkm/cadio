using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
        private ICommand _createPointCommand;
        private ICommand _removeSelectedObjectCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public Scene Scene
        {
            get { return _scene; }
            set
            {
                if (_scene != null)
                    ManageableScenes.Remove(_scene);

                _scene = value;
                ManageableScenes.Add(value);
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Scene> ManageableScenes { get; set; } = new ObservableCollection<Scene>(); 

        public WorldObject SelectedObject => Scene.Objects.FirstOrDefault(t => t.IsSelected);

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
            _scene.DetachObject(objectToRemove);
        }

        private Point3D GetInsertionLocation()
        {
            return _scene?.Manipulator?.Position ?? new Point3D(0, 0, 0);
        }
    }
}