using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes.Dynamic;
using CADio.Geometry.Shapes.Static;
using CADio.Helpers.MVVM;
using CADio.Mathematics.Numerical;
using CADio.SceneManagement.Interfaces;
using CADio.SceneManagement.Serialization;
using CADio.Views.DragDropSupport;

namespace CADio.SceneManagement
{
    public class InterpolatingBSplineObject : WorldObject, IDropzone, IControlPointDependent, ISmartEditTarget, ISaveable
    {
        private ICommand _requestSmartEditCommand;
        private ICommand _togglePolygonRenderingCommand;
        private bool _isPolygonRenderingEnabled;

        public ObservableCollection<ControlPoint> Objects { get; set; } 
            = new ObservableCollection<ControlPoint>();

        public bool IsSmartEditEnabled => SceneManager?.SmartEditTarget == this;

        public ICommand RequestSmartEditCommand
        {
            get { return _requestSmartEditCommand ?? (_requestSmartEditCommand = new RelayCommand(RequestSmartEdit)); }
            set { _requestSmartEditCommand = value; OnPropertyChanged(); }
        }

        public bool IsPolygonRenderingEnabled
        {
            get { return _isPolygonRenderingEnabled; }
            set
            {
                _isPolygonRenderingEnabled = value;
                SetupPolygonRendering();
                OnPropertyChanged();
            }
        }

        public ICommand TogglePolygonRenderingCommand
        {
            get { return _togglePolygonRenderingCommand ?? (_togglePolygonRenderingCommand = new RelayCommand(TogglePolygonRendering)); }
            set { _togglePolygonRenderingCommand = value; OnPropertyChanged(); }
        }

        private void TogglePolygonRendering()
        {
            IsPolygonRenderingEnabled = !IsPolygonRenderingEnabled;
        }

        private void SetupPolygonRendering()
        {
            if (!(Shape is BezierCurveC2))
                return;
            ((BezierCurveC2) Shape).IsPolygonRenderingEnabled = IsPolygonRenderingEnabled;
        }

        private void RequestSmartEdit()
        {
            SceneManager.SmartEditTarget = IsSmartEditEnabled ? null : this;
        }

        public void AttachObject(WorldObject worldObject)
        {
            worldObject.ObjectControlPointUsers.Add(this);
            Objects.Add(new ControlPoint() {Owner = this, Reference = worldObject});
        }

        public void DetachObjectReferences(WorldObject worldObject)
        {
            ControlPoint controlPoint;
            while ((controlPoint = Objects.FirstOrDefault(t => t.Reference == worldObject)) != null)
                Objects.Remove(controlPoint);
        }

        public void DetachControlPoint(ControlPoint controlPoint)
        {
            Objects.Remove(controlPoint);
        }

        public override void PrerenderUpdate()
        {
            var bezier = Shape as BezierCurveC2;
            if (bezier == null || Objects.Count < 4)
                return;

            var interpolationPoints = Objects.Select(t => t.Reference.Position).ToList();
            var interp = BSplineInterpolation3D.Interpolate(interpolationPoints);
            bezier.ControlPoints = interp.ControlPoints;
            bezier.CustomKnots = interp.Knots;
            bezier.ApplyParameterCorrection = false;
        }

        public override void Translate(Vector3D translation)
        {
            foreach (var obj in Objects.Select(t => t.Reference).Distinct().ToList())
                obj?.Translate(translation);
        }

        public void RegisterNewObject(WorldObject worldObject)
        {
            if (worldObject.Shape is MarkerPoint)
                AttachObject(worldObject);
        }

        public void NotifyAboutStateChange()
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged(nameof(IsSmartEditEnabled));
        }

        public void Drop(IUIDragable dragable)
        {
            var markerPoint = dragable as WorldObject;
            if (markerPoint?.Shape is MarkerPoint)
                AttachObject(markerPoint);
        }

        public override ICollection<ISceneSelectable> GetSelectableChildren()
        {
            return new List<ISceneSelectable>();
        }

        public void Save(SceneDataSaver saver)
        {
            saver.EmitObjectInfo(Scene.WorldObjectType.Interpolation, Name);

            foreach (var cp in Objects)
            {
                var id = saver.GetWorldObjectId(cp.Reference);
                saver.EmitInt(id);
            }

            saver.EmitObjectDataEnd();
        }
    }
}