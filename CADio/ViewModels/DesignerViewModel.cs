using System;
using System.Collections.Generic;
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
using CADio.Rendering;
using CADio.SceneManagement;
using Microsoft.Win32;

namespace CADio.ViewModels
{
    internal class DesignerViewModel : INotifyPropertyChanged
    {
        private Scene _scene = new Scene();
        private Renderer _renderer;
        private string _cursorInfo;
        private QualitySettingsViewModel _qualitySettingsViewModel = new QualitySettingsViewModel();
        private readonly Window _ownerWindow;

        private ICommand _newSceneCommand;
        private ICommand _loadSceneCommand;
        private ICommand _saveSceneCommand;
        private ICommand _saveSceneAsCommand;

        public QualitySettingsViewModel QualitySettingsViewModel
        {
            get { return _qualitySettingsViewModel; }
            set { _qualitySettingsViewModel = value; OnPropertyChanged(); }
        }

        public ICommand NewSceneCommand
        {
            get { return _newSceneCommand ?? (_newSceneCommand = new RelayCommand(RequestNewScene)); }
            set { _newSceneCommand = value; OnPropertyChanged(); }
        }

        public ICommand LoadSceneCommand
        {
            get { return _loadSceneCommand ?? (_loadSceneCommand = new RelayCommand(LoadScene)); }
            set { _loadSceneCommand = value; OnPropertyChanged(); }
        }

        public ICommand SaveSceneCommand
        {
            get { return _saveSceneCommand ?? (_saveSceneCommand = new RelayCommand(SaveScene)); }
            set { _saveSceneCommand = value; OnPropertyChanged(); }
        }

        public ICommand SaveSceneAsCommand
        {
            get { return _saveSceneAsCommand ?? (_saveSceneAsCommand = new RelayCommand(SaveSceneAs)); }
            set { _saveSceneAsCommand = value; OnPropertyChanged(); }
        }

        private void RequestNewScene()
        {
            var result = MessageBox.Show(_ownerWindow, "Creating new scene will clear this one. Are you sure?", "New scene confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
                return;

            CreateNewScene();
        }

        private void LoadScene()
        {
            var result = MessageBox.Show(_ownerWindow, "Loading a scene will clear this one. Are you sure?", "New scene confirmation",
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
            var manipulator = new WorldObject() {Shape = new Cursor3D(0.1), IsGrabable = false};
            _scene.AttachObject(manipulator);
            _scene.Manipulator = manipulator;
            _scene.PropertyChanged += SceneChanged;
            _scene.Objects.CollectionChanged += SceneCollectionChanged;
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

        public void GrabUsingMouse(Point screenSpaceClick, bool allowMultigrab, double limit = 0.2)
        {
            var selectables = GetSceneSelectables();

            if (selectables.Count == 0)
                return;

            ISceneSelectable closestObject = null;
            var closestDistance = double.MaxValue;

            foreach (var selectable in selectables)
            {
                var screenSpacePosition = ActiveRenderer.GetStandardScreenSpacePosition(selectable.Position);
                if (!screenSpacePosition.HasValue) continue;
                var candidateLength = (screenSpacePosition.Value - screenSpaceClick).Length;
                if (candidateLength >= closestDistance) continue;
                closestObject = selectable;
                closestDistance = candidateLength;
            }

            if (closestObject == null || closestDistance > limit)
                return;

            if (allowMultigrab) _scene.ToggleObjectGrab(closestObject);
            else _scene.SetObjectGrab(closestObject);
        }

        public void GrabNearestPoint(bool allowMultigrab, double limit = 1.0)
        {
            var selectables = GetSceneSelectables();

            ISceneSelectable closestSelectable = null;
            var closestDistance = double.MaxValue;

            foreach (var t in selectables)
            {
                if (t.IsGrabbed && allowMultigrab) continue;
                var candidateDistance = (t.Position - _scene.Manipulator.Position).Length;
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
    }
}
