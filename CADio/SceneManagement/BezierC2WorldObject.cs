using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes.Dynamic;
using CADio.Geometry.Shapes.Static;
using CADio.Helpers.MVVM;
using CADio.Mathematics.Numerical;
using CADio.Views.DragDropSupport;

namespace CADio.SceneManagement
{
    public class BezierC2WorldObject : WorldObject, IDropzone, IControlPointDependent, ISmartEditTarget, ISaveable
    {
        private ICommand _requestSmartEditCommand;
        private ICommand _togglePolygonRenderingCommand;
        private bool _isPolygonRenderingEnabled;

        private List<BSplineBernsteinVirtualPoint> _virtualPoints = new List<BSplineBernsteinVirtualPoint>(); 

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
            if (bezier == null)
                return;

            bezier.ControlPoints = Objects.Select(t => t.Reference.Position).ToList();

            if (bezier.ControlPoints.Count > 3)
            {
                var bernstein = BSplineToBernsteinConverter.ConvertAssumingEquidistantKnots(bezier.ControlPoints);

                if (_virtualPoints.Count == bernstein.Count)
                {
                    for (var i = 0; i < _virtualPoints.Count; ++i)
                        _virtualPoints[i].Position = bernstein[i];
                }
                else
                {
                    _virtualPoints =
                        bernstein.Select(t => new BSplineBernsteinVirtualPoint(this) {Position = t}).ToList();
                }
            }
            else
                _virtualPoints.Clear();
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
            var bezierCurveC2 = ((BezierCurveC2)Shape);
            if (bezierCurveC2.Basis == CurveBasis.BSpline)
                return new List<ISceneSelectable>();

            return _virtualPoints.Cast<ISceneSelectable>().ToList();
        }

        public void RequestMovement(BSplineBernsteinVirtualPoint bSplineBernsteinVirtualPoint, Vector3D translation)
        {
            var index = _virtualPoints.IndexOf(bSplineBernsteinVirtualPoint);
            if (index == -1)
                return;

            var shape = (BezierCurveC2) Shape;
            if (shape.Basis == CurveBasis.BSpline)
                return;

            var segmentId = index/3; // common node is included to previous

            if (index%3 == 0)
            {
                Objects[segmentId + 1].Reference.Position += translation;
            }
            else
            {
                var left = Objects[segmentId + 1].Reference;
                var right = Objects[segmentId + 2].Reference;

                var i = index%3;
                var newBernsteinPos = bSplineBernsteinVirtualPoint.Position + translation;
                right.Position = (Point3D)((3.0/i)*(Vector3D)(newBernsteinPos - (1.0-i/3.0)*(Vector3D)left.Position));
            }
        }

        public void Save(Scene.SceneDataGatherer gatherer)
        {
            gatherer.EmitObjectInfo(Scene.WorldObjectType.BSplineCurve, Name);
            gatherer.EmitInt(Objects.Count);

            foreach (var cp in Objects)
            {
                var id = gatherer.GetWorldObjectId(cp.Reference);
                gatherer.EmitInt(id);
            }

            gatherer.EmitObjectDataEnd();
        }
    }
}