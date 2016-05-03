using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes;
using CADio.Geometry.Shapes.Dynamic;
using CADio.Geometry.Shapes.Static;
using CADio.Helpers.MVVM;
using CADio.SceneManagement;

namespace CADio.ViewModels
{
    internal class SceneTreeViewModel : INotifyPropertyChanged
    {
        private static int _sessionPointId = 1;
        private static int _sessionBezierId = 1;

        private Scene _scene;
        private ICommand _createPointCommand;
        private ICommand _removeSelectedObjectCommand;
        private ICommand _createBezierCurveC0Command;
        private ICommand _createBezierCurveC2Command;
        private ICommand _createInterpolatingBSplineCommand;
        private ICommand _createBezierSurfaceCommand;

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

        public ICommand CreateBezierCurveC0Command
        {
            get { return _createBezierCurveC0Command ?? (_createBezierCurveC0Command = new RelayCommand(CreateBezierCurveC0)); }
            set { _createBezierCurveC0Command = value; OnPropertyChanged(); }
        }

        public ICommand CreateBezierCurveC2Command
        {
            get { return _createBezierCurveC2Command ?? (_createBezierCurveC2Command = new RelayCommand(CreateBezierCurveC2)); }
            set { _createBezierCurveC2Command = value; OnPropertyChanged(); }
        }

        public ICommand CreateInterpolatingBSplineCommand
        {
            get { return _createInterpolatingBSplineCommand ?? (_createInterpolatingBSplineCommand = new RelayCommand(CreateInterpolatingBSpline)); }
            set { _createInterpolatingBSplineCommand = value; OnPropertyChanged(); }
        }

        public ICommand CreateBezierSurfaceCommand
        {
            get { return _createBezierSurfaceCommand ?? (_createBezierSurfaceCommand = new RelayCommand(CreateBezierSurface)); }
            set { _createBezierSurfaceCommand = value; OnPropertyChanged(); }
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
                Name = "Point #" + _sessionPointId++,
                Position = GetInsertionLocation(),
                Shape = new MarkerPoint(),
            };

            _scene.SmartEditTarget?.RegisterNewObject(newPoint);
            _scene.AttachObject(newPoint);
        }

        private void CreateBezierCurveC0()
        {
            var bezier = new BezierC0WorldObject()
            {
                Name = "Bezier Curve C0 #" + _sessionBezierId++,
                Shape = new BezierCurveC0(),
            };

            _scene.SmartEditTarget?.RegisterNewObject(bezier);
            _scene.AttachObject(bezier);
        }

        private void CreateBezierCurveC2()
        {
            var bezier = new BezierC2WorldObject()
            {
                Name = "Bezier Curve C2 #" + _sessionBezierId++,
                Shape = new BezierCurveC2(),
            };

            _scene.SmartEditTarget?.RegisterNewObject(bezier);
            _scene.AttachObject(bezier);
        }

        private void CreateInterpolatingBSpline()
        {
            var bezier = new InterpolatingBSplineObject()
            {
                Name = "InterpolatingBSplineObject #" + _sessionBezierId++,
                Shape = new BezierCurveC2(),
            };

            _scene.SmartEditTarget?.RegisterNewObject(bezier);
            _scene.AttachObject(bezier);
        }

        private void CreateBezierSurface()
        {
            var viewModel = BezierSurfaceCreationViewModel.ShowDialog();
            if (viewModel == null)
                return;

            var surface = new BezierSurfaceWorldObject(viewModel.SegmentsX, viewModel.SegmentsY)
            {
                Name = "Bezier Surface",
                Shape = new BezierPatchGroup(),
            };

            _scene.SmartEditTarget?.RegisterNewObject(surface);
            _scene.AttachObject(surface);
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