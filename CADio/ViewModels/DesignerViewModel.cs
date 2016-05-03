using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes;
using CADio.Geometry.Shapes.Dynamic;
using CADio.Geometry.Shapes.Static;
using CADio.Rendering;
using CADio.SceneManagement;

namespace CADio.ViewModels
{
    internal class DesignerViewModel : INotifyPropertyChanged
    {
        private readonly Scene _scene = new Scene();
        private Renderer _renderer;
        private string _cursorInfo;

        public DesignerViewModel()
        {
            var manipulator = new WorldObject() {Shape = new Cursor3D(0.1), IsGrabable = false};

            // todo: move bezier into appropriate place and fix dynamic behaviour
            /*var pt1 = new WorldObject {Position = new Point3D(-1, 0.2, 0), Shape = new MarkerPoint()};
            var pt2 = new WorldObject {Position = new Point3D(-0.5, 0.1, 0), Shape = new MarkerPoint()};
            var pt3 = new WorldObject {Position = new Point3D(0, 0, 0), Shape = new MarkerPoint()};
            var pt4 = new WorldObject {Position = new Point3D(0.5, 0.3, 0), Shape = new MarkerPoint()};
            var pt5 = new WorldObject { Position = new Point3D(1, 0, 0), Shape = new MarkerPoint() };

            var bezier = new InterpolatingBSplineObject() {Shape = new BezierCurveC2()};
            bezier.AttachObject(pt1);
            bezier.AttachObject(pt2);
            bezier.AttachObject(pt3);
            bezier.AttachObject(pt4);
            bezier.AttachObject(pt5);

            _scene.AttachObject(pt1);
            _scene.AttachObject(pt2);
            _scene.AttachObject(pt3);
            _scene.AttachObject(pt4);
            _scene.AttachObject(pt5);

            _scene.AttachObject(bezier);*/
            _scene.AttachObject(manipulator);
            _scene.Manipulator = manipulator;
            _scene.PropertyChanged += SceneChanged;
            _scene.Objects.CollectionChanged += SceneCollectionChanged;

            ActiveRenderer = new Renderer {Scene = _scene};
            SceneTreeViewModel = new SceneTreeViewModel() {Scene = _scene};

            UpdateManipulatorInfo();
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
            _scene.GrabbedObject?.Translate(shift);

            UpdateManipulatorInfo();
            ActiveRenderer.ForceRedraw();
        }

        public void GrabUsingMouse(Point screenSpaceClick, double limit = 0.2)
        {
            var selectables = GetSceneSelectables();

            if (selectables.Count == 0)
                return;

            ISceneSelectable closestObject = null;
            double closestDistance = double.MaxValue;

            foreach (var selectable in selectables)
            {
                var screenSpacePosition = ActiveRenderer.GetStandardScreenSpacePosition(selectable.Position);
                if (!screenSpacePosition.HasValue) continue;
                var candidateLength = (screenSpacePosition.Value - screenSpaceClick).Length;
                if (candidateLength >= closestDistance) continue;
                closestObject = selectable;
                closestDistance = candidateLength;
            }

            if (closestDistance > limit) return;

            TryToggleGrabbedObject(closestObject);
        }

        public void GrabNearestPointOrUngrab(double limit = 1.0)
        {
            var markerPoints = GetSceneSelectables();

            var closestPoint = markerPoints.Count > 0 ? markerPoints[0] : null;
            var closestDistance = closestPoint != null
                ? (closestPoint.Position - _scene.Manipulator.Position).Length
                : 0.0;

            for (var i = 1; i < markerPoints.Count; ++i)
            {
                var candidateDistance = (markerPoints[i].Position - _scene.Manipulator.Position).Length;
                if (candidateDistance >= closestDistance) continue;
                closestPoint = markerPoints[i];
                closestDistance = candidateDistance;
            }

            if (closestDistance > limit)
            {
                _scene.GrabbedObject = null;
                return;
            }

            TryToggleGrabbedObject(closestPoint);
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

        private void TryToggleGrabbedObject(ISceneSelectable closestPoint)
        {
            var grabbedObject = _scene.GrabbedObject != closestPoint ? closestPoint : null;
            if (grabbedObject != null)
                _scene.Manipulator.Position = grabbedObject.Position;
            _scene.GrabbedObject = grabbedObject;
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
    }
}
