using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes.Dynamic;
using CADio.Helpers.MVVM;

namespace CADio.SceneManagement
{
    public class BSplineSurfaceWorldObject : WorldObject
    {
        private int _segmentsX;
        private int _segmentsY;
        private int _rowLength;
        private bool _isPolygonRenderingEnabled;
        private ICommand _togglePolygonRenderingCommand;
        private readonly List<VirtualPoint> _virtualPoints = new List<VirtualPoint>();

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

        public BSplineSurfaceWorldObject()
        {
            Shape = new BSplinePatchGroup();
        }

        public static BSplineSurfaceWorldObject CreateFlatGrid(int segmentsX, int segmentsY, double width, double height)
        {
            var surface = new BSplineSurfaceWorldObject
            {
                _segmentsX = segmentsX,
                _segmentsY = segmentsY,
                _rowLength = 3+segmentsX,
            };

            surface.SetupVirtualPointsGrid(3 + segmentsX, 3 + segmentsY, width, height);
            return surface;
        }

        public static BSplineSurfaceWorldObject CreateCylindrical(int segmentsX, int segmentsY,
            double radius, double height)
        {
            var surface = new BSplineSurfaceWorldObject
            {
                _segmentsX = segmentsX,
                _segmentsY = segmentsY,
                _rowLength = 3+segmentsX-3, // todo: min segments = 4 <= 3+segmentsX-3 ==> segmentsX >= 4 
            };

            surface.SetupVirtualPointsCylinder(surface._rowLength, 3+segmentsY, radius, height);
            return surface;
        }

        public override void PrerenderUpdate()
        {
            var patch = Shape as BSplinePatchGroup;
            if (patch == null)
                return;

            patch.SegmentsX = _segmentsX;
            patch.SegmentsY = _segmentsY;
            patch.ControlPointsRowLength = _rowLength;
            patch.ControlPoints = _virtualPoints.Select(t => t.Position).ToList();
        }

        public override ICollection<ISceneSelectable> GetSelectableChildren()
        {
            return _virtualPoints.Cast<ISceneSelectable>().ToList();
        }

        private void SetupPolygonRendering()
        {
            if (!(Shape is BSplinePatchGroup))
                return;
            ((BSplinePatchGroup)Shape).IsPolygonRenderingEnabled = IsPolygonRenderingEnabled;
        }

        private void SetupVirtualPointsGrid(int cols, int rows, double width = 1, double height = 1)
        {
            var spacingX = 1.0 / (cols - 1);
            var spacingY = 1.0 / (rows - 1);
            var totalX = spacingX * (cols - 1);
            var totalY = spacingY * (rows - 1);

            _virtualPoints.Clear();

            for (var y = 0; y < rows; y++)
            {
                for (var x = 0; x < cols; x++)
                {
                    _virtualPoints.Add(new VirtualPoint()
                    {
                        Position = new Point3D(width * (x * spacingX - totalX * 0.5), 0, height * (y * spacingY - totalY * 0.5))
                    });
                }
            }
        }

        private void SetupVirtualPointsCylinder(int onCrossSectionPoints, int onLengthPoints, double radius, double height)
        {
            _virtualPoints.Clear();

            for (var i = 0; i < onLengthPoints; ++i)
            {
                var h = height*i/onLengthPoints;
                for (var j = 0; j < onCrossSectionPoints; ++j)
                {
                    var angle = 2*Math.PI*j/onCrossSectionPoints;
                    _virtualPoints.Add(new VirtualPoint()
                    {
                        Position = new Point3D(Math.Sin(angle), h, Math.Cos(angle)),
                    });
                }
            }
        }
    }
}