using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes.Dynamic;
using CADio.Geometry.Shapes.Static;
using CADio.Helpers.MVVM;
using CADio.SceneManagement.Interfaces;
using CADio.Views.DragDropSupport;

namespace CADio.SceneManagement
{
    public class BezierC0WorldObject : 
        WorldObject, 
        IDropzone, 
        ISmartEditTarget, 
        IControlPointDependent, 
        ISaveable
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
            if (!(Shape is BezierCurveC0))
                return;
            ((BezierCurveC0) Shape).IsPolygonRenderingEnabled = IsPolygonRenderingEnabled;
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
            var bezier = Shape as BezierCurveC0;
            if (bezier == null)
                return;

            bezier.ControlPoints = Objects.Select(t => t.Reference.Position).ToList();
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
            OnPropertyChanged(nameof(IsSmartEditEnabled));
        }

        public void Drop(IUIDragable dragable)
        {
            var markerPoint = dragable as WorldObject;
            if (markerPoint?.Shape is MarkerPoint)
                AttachObject(markerPoint);
        }

        public void Save(Scene.SceneDataGatherer gatherer)
        {
            gatherer.EmitObjectInfo(Scene.WorldObjectType.BezierCurve, Name);
            gatherer.EmitInt(Objects.Count);
            gatherer.EmitEOL();

            foreach (var cp in Objects)
            {
                var id = gatherer.GetWorldObjectId(cp.Reference);
                gatherer.EmitInt(id);
            }

            gatherer.EmitObjectDataEnd();
        }
    }
}