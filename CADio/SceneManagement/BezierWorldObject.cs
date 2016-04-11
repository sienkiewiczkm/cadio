using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes.Dynamic;
using CADio.Geometry.Shapes.Static;
using CADio.Helpers.MVVM;

namespace CADio.SceneManagement
{
    public class BezierWorldObject : WorldObject, ISmartEditTarget
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
            if (!(Shape is SegmentedBezier))
                return;
            ((SegmentedBezier) Shape).IsPolygonRenderingEnabled = IsPolygonRenderingEnabled;
        }

        private void RequestSmartEdit()
        {
            SceneManager.SmartEditTarget = IsSmartEditEnabled ? null : this;
        }

        public void AttachObject(WorldObject worldObject)
        {
            //if (Objects.Any(t => t.Reference == worldObject))
            //    return;
            worldObject.ObjectUsers.Add(this);
            Objects.Add(new ControlPoint() {Owner = this, Reference = worldObject});
        }

        public void DetachObject(WorldObject worldObject)
        {
            ControlPoint controlPoint;
            while ((controlPoint = Objects.FirstOrDefault(t => t.Reference == worldObject)) != null)
                Objects.Remove(controlPoint);
        }

        public void DetachObject(ControlPoint controlPoint)
        {
            Objects.Remove(controlPoint);
            //if (Objects.All(t => t.Reference != controlPoint.Reference))
            //    controlPoint.Re
        }

        public override void PrerenderUpdate()
        {
            var bezier = Shape as SegmentedBezier;
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
    }
}