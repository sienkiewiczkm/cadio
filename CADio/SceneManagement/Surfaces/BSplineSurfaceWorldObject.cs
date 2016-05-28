using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.Configuration;
using CADio.Geometry.Shapes.Dynamic;
using CADio.Helpers.MVVM;
using CADio.SceneManagement.Interfaces;

namespace CADio.SceneManagement.Surfaces
{
    public class BSplineSurfaceWorldObject : WorldObject, ISaveable
    {
        private int _segmentsX;
        private int _segmentsY;
        private int _rowLength;
        private bool _isPolygonRenderingEnabled;
        private ICommand _togglePolygonRenderingCommand;
        private readonly List<VirtualPoint> _virtualPoints 
            = new List<VirtualPoint>();
        private bool _folded;

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
                _folded = false,
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
                _folded = true,
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
            patch.ControlPoints = _virtualPoints
                .Select(t => new SurfaceControlPoint() {
                    Position = t.Position,
                    ColorOverride = t.IsGrabbed 
                        ? ColorSettings.SceneSelection
                        : (Color?) null
                }).ToList();
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
                        Position = new Point3D(radius*Math.Sin(angle), h, radius*Math.Cos(angle)),
                    });
                }
            }
        }

        public void Save(Scene.SceneDataGatherer gatherer)
        {
            var totalRows = _virtualPoints.Count / _rowLength;

            gatherer.EmitObjectInfo(Scene.WorldObjectType.BSplineSurface, Name);
            gatherer.EmitInt(totalRows);
            gatherer.EmitInt(_rowLength);
            gatherer.EmitChar(_folded ? 'C' : 'R');
            gatherer.EmitChar('V');
            gatherer.EmitEOL();

            for (var row = 0; row < totalRows; ++row)
            {
                for (var column = 0; column < _rowLength; ++column)
                {
                    var dataRow = row;
                    var dataColumn = column;
                    var pos = _virtualPoints[dataColumn + dataRow * _rowLength].Position;
                    gatherer.EmitInt(gatherer.CreateReferencePoint(pos));
                }
            }

            gatherer.EmitObjectDataEnd();
        }

        public void BuildFromExternalData(Point3D[,] data, bool folded)
        {
            var rows = data.GetLength(0);
            _rowLength = data.GetLength(1);
            _folded = folded;

            if (!folded)
            {
                // 3 + segmentsX, 3 + segmentsY
                _segmentsX = _rowLength - 3;
                _segmentsY = rows - 3;
            }
            else
            {
                // surface._rowLength, 3+segmentsY
                _segmentsX = _rowLength;
                _segmentsY = rows - 3;
            }

            _virtualPoints.Clear();

            for (var y = 0; y < rows; y++)
            {
                for (var x = 0; x < _rowLength; x++)
                {
                    _virtualPoints.Add(new VirtualPoint()
                    {
                        Position = data[y, x]
                    });
                }
            }
        }
    }
}