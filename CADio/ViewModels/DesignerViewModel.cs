using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using CADio.Configuration;
using CADio.Geometry.Shapes;
using CADio.Geometry.Shapes.Dynamic;
using CADio.Geometry.Shapes.Static;
using CADio.Helpers.MVVM;
using CADio.Mathematics.Interfaces;
using CADio.Mathematics.Intersections;
using CADio.Mathematics.Surfaces;
using CADio.Rendering;
using CADio.SceneManagement;
using CADio.SceneManagement.Interfaces;
using CADio.SceneManagement.Surfaces;
using CADio.Views;
using Microsoft.Win32;

namespace CADio.ViewModels
{
    internal class DesignerViewModel : INotifyPropertyChanged
    {
        private Scene _scene = new Scene();
        private Renderer _renderer;
        private string _cursorInfo;
        private QualitySettingsViewModel _qualitySettingsViewModel 
            = new QualitySettingsViewModel();
        private readonly Window _ownerWindow;

        private ICommand _newSceneCommand;
        private ICommand _loadSceneCommand;
        private ICommand _saveSceneCommand;
        private ICommand _saveSceneAsCommand;
        private ICommand _collapseSelectionCommand;
        private ICommand _fillWithGregoryCommand;
        private RelayCommand _intersectSurfacesCommand;

        public QualitySettingsViewModel QualitySettingsViewModel
        {
            get { return _qualitySettingsViewModel; }
            set
            {
                _qualitySettingsViewModel = value;
                OnPropertyChanged();
            }
        }

        public ICommand NewSceneCommand
        {
            get
            {
                return _newSceneCommand ?? 
                    (_newSceneCommand = new RelayCommand(RequestNewScene));
            }
            set
            {
                _newSceneCommand = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadSceneCommand
        {
            get
            {
                return _loadSceneCommand ?? 
                    (_loadSceneCommand = new RelayCommand(LoadScene));
            }
            set
            {
                _loadSceneCommand = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveSceneCommand
        {
            get
            {
                return _saveSceneCommand ?? 
                    (_saveSceneCommand = new RelayCommand(SaveScene));
            }
            set
            {
                _saveSceneCommand = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveSceneAsCommand
        {
            get
            {
                return _saveSceneAsCommand ?? 
                    (_saveSceneAsCommand = new RelayCommand(SaveSceneAs));
            }
            set
            {
                _saveSceneAsCommand = value;
                OnPropertyChanged();
            }
        }

        public ICommand CollapseSelectionCommand
        {
            get
            {
                return _collapseSelectionCommand ?? 
                    (_collapseSelectionCommand 
                        = new RelayCommand(
                            CollapseSelection, 
                            () => IsSelectionCollapsable)
                    );
            }
            set
            {
                _collapseSelectionCommand = value;
                OnPropertyChanged();
            }
        }

        public ICommand FillWithGregoryCommand
        {
            get
            {
                if (_fillWithGregoryCommand == null)
                    _fillWithGregoryCommand = new RelayCommand(
                        FillWithGregory,
                        IsGregoryPatchAvailable
                    );
                return _fillWithGregoryCommand;
            }
        }

        public ICommand IntersectSurfacesCommand
        {
            get
            {
                if (_intersectSurfacesCommand == null)
                    _intersectSurfacesCommand = new RelayCommand(
                        IntersectSurfaces
                    );
                return _intersectSurfacesCommand;
            }
        }

        private void IntersectSurfaces()
        {
            var surfaces = _scene.GrabbedObjects
                .Where(t => t is IParametrizationQueryable)
                .Cast<IParametrizationQueryable>()
                .ToList();

            var firstPatch = surfaces[0].GetParametricSurface();
            var secondPatch = surfaces[1].GetParametricSurface();

            var nearestFinder = new SurfaceSamplingNearestFinder();
            nearestFinder.SamplesU = 64;
            nearestFinder.SamplesV = 64;

            var referencePosition = _scene.Manipulator.Position;

            var finder = new DomainIntersectionFinder();
            finder.NearestPointFinder = nearestFinder;

            var polygon = finder.FindIntersectionPolygon(
                firstPatch,
                secondPatch,
                referencePosition
            );

            var firstParametrisationWindow = new ParametricPreview();
            var secondParametrisationWindow = new ParametricPreview();

            firstParametrisationWindow.ViewModel.ParametricPolygon
                = new ObservableCollection<Parametrisation>(
                    polygon.Polygon.Select(t => t.First).ToList());

            secondParametrisationWindow.ViewModel.ParametricPolygon
                = new ObservableCollection<Parametrisation>(
                    polygon.Polygon.Select(t => t.Second).ToList());

            firstParametrisationWindow.ViewModel.Name = surfaces[0].Name;
            secondParametrisationWindow.ViewModel.Name = surfaces[1].Name;

            firstParametrisationWindow.ViewModel.RedrawPreview();
            secondParametrisationWindow.ViewModel.RedrawPreview();

            firstParametrisationWindow.Show();
            secondParametrisationWindow.Show();
        }

        private bool IsGregoryPatchAvailable()
        {
            return _scene.GrabbedObjects
                .Count(t => t is BezierSurfaceWorldObject) >= 3;
        }

        private void FillWithGregory()
        {
            var gregoryAdjacentPatches = _scene.GrabbedObjects
                .Where(t => t is BezierSurfaceWorldObject)
                .Cast<BezierSurfaceWorldObject>()
                .Take(3) // todo: support for more
                .ToList();

            var outline = BezierSurfaceWorldObject.FindHoleOutline(
                gregoryAdjacentPatches
            );

            if (outline.Count != 9)
            {
                MessageBox.Show(
                    _ownerWindow,
                    "Hole cannot be filled for selected patches",
                    "Operation cannot be performed",
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information
                );
                return;
            }

            var fill = new TriangularHoleFill {OutlinePoints = outline};
            _scene.AttachObject(fill);
        }

        private void RequestNewScene()
        {
            var result = MessageBox.Show(
                _ownerWindow, 
                "Creating new scene will clear this one. Are you sure?",
                "New scene confirmation",
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.No)
                return;

            CreateNewScene();
        }

        private void LoadScene()
        {
            var result = MessageBox.Show(_ownerWindow, "Loading a scene will clear this one. Are you sure?",
                "New scene confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
                return;

            var ofd = new OpenFileDialog();
            var fileResult = ofd.ShowDialog();
            if (fileResult == false)
                return;

            CreateNewScene();
            _scene.Import(ofd.FileName);
            ActiveRenderer.ForceRedraw();
        }

        private void SaveScene()
        {
            if (_scene.Filename == null)
            {
                SaveSceneAs();
                return;
            }

            _scene.Save();
        }

        private void SaveSceneAs()
        {
            var sfd = new SaveFileDialog();
            var fileResult = sfd.ShowDialog();
            if (fileResult == false)
                return;

            _scene.Filename = sfd.FileName;
            _scene.Save();
        }

        private bool IsSelectionCollapsable => _scene.GrabbedObjects.Count > 1;
        private void CollapseSelection()
        {
            _scene.CollapseSelection();
        }

        public DesignerViewModel(Window ownerWindow)
        {
            _ownerWindow = ownerWindow;
            QualitySettingsViewModel.PropertyChanged += RedrawOnPropertyChanged;
            GlobalSettings.QualitySettingsViewModel = QualitySettingsViewModel;

            ActiveRenderer = new Renderer();
            SceneTreeViewModel = new SceneTreeViewModel();

            CreateNewScene();
        }

        private void CreateNewScene()
        {
            _scene = new Scene();
            var manipulator = new WorldObject()
            {
                Shape = new Cursor3D(0.1),
                IsGrabable = false
            };

            _scene.AttachObject(manipulator);
            _scene.Manipulator = manipulator;
            _scene.PropertyChanged += SceneChanged;
            _scene.Objects.CollectionChanged += SceneCollectionChanged;

            /*
            var bezier = BezierSurfaceWorldObject.CreateFlatGrid(1, 1, 2, 2);
            bezier.Position = new Point3D(-2, 0.0f, 0.0f);
            var gregory = new GregoryPatchWorldObject();
            gregory.GregoryPatchShape.ObservedBezierDebug = bezier;

            _scene.AttachObject(gregory);
            _scene.AttachObject(bezier);
            */

            ActiveRenderer.Scene = _scene;
            SceneTreeViewModel.Scene = _scene;
            UpdateManipulatorInfo();
        }

        private void RedrawOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ActiveRenderer.ForceRedraw();
        }

        private void SceneChanged(object sender, PropertyChangedEventArgs e)
        {
            ActiveRenderer.ForceRedraw();
        }

        private void SceneCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ActiveRenderer.ForceRedraw();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Renderer ActiveRenderer
        {
            get { return _renderer; }
            set
            {
                _renderer = value;
                OnPropertyChanged();
            }
        }

        public double EyeDistance
        {
            get {  return ActiveRenderer.EyeDistance; }
            set
            {
                ActiveRenderer.EyeDistance = value; // todo: notifiers
                ActiveRenderer.ForceRedraw();
                OnPropertyChanged();
            }
        }

        public string CursorReadableInfo
        {
            get { return _cursorInfo; }
            set { _cursorInfo = value; OnPropertyChanged(); }
        }

        public SceneTreeViewModel SceneTreeViewModel { get; set; }

        public void RotateSceneWithMouse(Vector mouseMovement)
        {
            var rotation = new Vector3D(-mouseMovement.Y/50, -mouseMovement.X/50, 0);
            ActiveRenderer.Scene.Camera.Rotate(rotation);

            UpdateManipulatorInfo();
            ActiveRenderer.ForceRedraw();
        }

        public void ScaleWithMouse(int delta)
        {
            ActiveRenderer.Scene.Camera.Zoom += delta*0.002;

            UpdateManipulatorInfo();
            ActiveRenderer.ForceRedraw();
        }

        public void TranslateSceneWithMouse(Vector mouseMovement)
        {
            ActiveRenderer.Scene.Camera.Move(new Vector3D(mouseMovement.X/50, 0, -mouseMovement.Y/50));

            UpdateManipulatorInfo();
            ActiveRenderer.ForceRedraw();
        }

        public void MoveManipulator(double dx, double dy, double dz)
        {
            var shift = new Vector3D(dx, dy, dz);
            _scene.Manipulator.Translate(shift);
            foreach (var obj in _scene.GrabbedObjects)
                obj.Translate(shift);

            UpdateManipulatorInfo();
            ActiveRenderer.ForceRedraw();
        }

        public void GrabUsingMouse(
            Point screenSpaceClick, 
            bool allowMultigrab, 
            double limit = 0.2
            )
        {
            var selectables = GetSceneSelectables();
            if (selectables.Count == 0)
                return;

            ISceneSelectable closestObject = null;
            var closestDistance = double.MaxValue;

            foreach (var selectable in selectables)
            {
                var screenSpacePosition = ActiveRenderer
                    .GetStandardScreenSpacePosition(selectable.WorldPosition);
                if (!screenSpacePosition.HasValue) continue;
                var candidateLength = 
                    (screenSpacePosition.Value - screenSpaceClick).Length;
                if (candidateLength >= closestDistance) continue;
                closestObject = selectable;
                closestDistance = candidateLength;
            }

            if (closestObject == null || closestDistance > limit)
                return;

            if (allowMultigrab) _scene.ToggleObjectGrab(closestObject);
            else _scene.SetObjectGrab(closestObject);
        }

        public void GrabUsingMouseRect(
            Rect screenSpaceGrabRect,
            bool allowMultigrab
            )
        {
            var selectables = GetSceneSelectables();
            if (selectables.Count == 0)
                return;

            foreach (var selectable in selectables)
            {
                var screenSpacePosition =
                    ActiveRenderer.GetStandardScreenSpacePosition(
                        selectable.WorldPosition
                    );
                if (!screenSpacePosition.HasValue) continue;
                if (screenSpaceGrabRect.Contains(screenSpacePosition.Value))
                    _scene.GrabObject(selectable);
            }
        }

        public void GrabNearestPoint(bool allowMultigrab, double limit = 1.0)
        {
            var selectables = GetSceneSelectables();

            ISceneSelectable closestSelectable = null;
            var closestDistance = double.MaxValue;

            foreach (var t in selectables)
            {
                if (t.IsGrabbed && allowMultigrab) continue;
                var candidateDistance = 
                    (t.Position - _scene.Manipulator.Position).Length;
                if (candidateDistance >= closestDistance) continue;
                closestSelectable = t;
                closestDistance = candidateDistance;
            }

            if (closestSelectable == null || closestDistance > limit)
                return;

            if (allowMultigrab) _scene.ToggleObjectGrab(closestSelectable);
            else _scene.SetObjectGrab(closestSelectable);
        }

        private IList<ISceneSelectable> GetSceneSelectables()
        {
            var markerPoints = _scene.Objects
                .Where(t => t.Shape is MarkerPoint)
                .Cast<ISceneSelectable>()
                .ToList();

            var selectableChildren = _scene.Objects
                .SelectMany(t => t.GetSelectableChildren())
                .ToList();

            return markerPoints.Union(selectableChildren).ToList();
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

        private void UpdateManipulatorInfo()
        {
            var cursorScreenPos = ActiveRenderer.GetStandardScreenSpacePosition(_scene.Manipulator.Position);
            var cursorViewPosInfo = cursorScreenPos.HasValue
                ? String.Format("Cursor screen position: ({0:0.##} ; {1:0.##})", cursorScreenPos.Value.X,
                    cursorScreenPos.Value.Y)
                : "Cursor screen position: <Clipped>";

            CursorReadableInfo = String.Format("Cursor world position: ({0:0.##} ; {1:0.##} ; {2:0.##}) {3}",
                _scene.Manipulator.Position.X, _scene.Manipulator.Position.Y, _scene.Manipulator.Position.Z,
                cursorViewPosInfo);
        }

        public void UngrabSelection()
        {
            _scene.UngrabAllObjects();
        }

        public void RotateSelectionZAxis(int rotationStep)
        {
            var alpha = rotationStep*(1/(36*Math.PI));
            var cosa = Math.Cos(alpha);
            var sina = Math.Sin(alpha);

            var centerMass = GetSelectionCenterMass();

            foreach (var grabbed in _scene.GrabbedObjects)
            {
                var oldPos = grabbed.Position - centerMass;
                var x = oldPos.X*cosa - oldPos.Y*sina;
                var y = oldPos.X*sina + oldPos.Y*cosa;
                var z = oldPos.Z;
                grabbed.Position = centerMass + new Vector3D(x, y, z);
            }
        }

        public void ScaleSelection(int scalingStep)
        {
            var factor = 1+scalingStep*0.05;
            var centerMass = GetSelectionCenterMass();
            foreach (var grabbed in _scene.GrabbedObjects)
            {
                var positionVector = grabbed.Position - centerMass;
                grabbed.Position = centerMass + positionVector*factor;
            }
        }

        public Point3D GetSelectionCenterMass()
        {
            var center = new Vector3D(0.0f, 0.0f, 0.0f);
            var divider = 0;

            foreach (var grabbed in _scene.GrabbedObjects)
            {
                center += (Vector3D) grabbed.Position;
                divider++;
            }

            return (Point3D)(center/divider);
        }
    }
}
