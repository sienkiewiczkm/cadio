using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.Configuration;
using CADio.Geometry.Generators;
using CADio.Geometry.Shapes.Dynamic;
using CADio.Helpers.MVVM;
using CADio.Mathematics.Interfaces;
using CADio.Mathematics.Surfaces;
using CADio.SceneManagement.Interfaces;
using CADio.SceneManagement.Points;
using CADio.SceneManagement.Serialization;

namespace CADio.SceneManagement.Surfaces
{
    public class BSplineSurfaceWorldObject : 
        WorldObject, 
        ISaveable,
        IParametrizationQueryable
    {
        private int _segmentsU;
        private int _segmentsV;
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
            get
            {
                return _togglePolygonRenderingCommand ?? 
                    (_togglePolygonRenderingCommand = 
                        new RelayCommand(TogglePolygonRendering));
            }
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

        public static BSplineSurfaceWorldObject CreateFlatGrid(
            int segmentsU, 
            int segmentsV, 
            double width, 
            double height
            )
        {
            var surface = new BSplineSurfaceWorldObject
            {
                _segmentsU = segmentsU,
                _segmentsV = segmentsV,
                _folded = false,
            };

            surface.SetupVirtualPointsGrid(
                3 + segmentsU, 
                3 + segmentsV, 
                width, 
                height
            );

            return surface;
        }

        public static BSplineSurfaceWorldObject CreateCylindrical(
            int segmentsX, 
            int segmentsY,
            double radius, 
            double height
            )
        {
            var surface = new BSplineSurfaceWorldObject
            {
                _segmentsU = segmentsX,
                _segmentsV = segmentsY,
                // todo: min segments = 4 <= 3+segmentsU-3 ==> segmentsU >= 4 
                _folded = true,
            };

            surface.SetupVirtualPointsCylinder(
                3+segmentsX,
                segmentsY, 
                radius, 
                height
            );

            return surface;
        }

        public override void PrerenderUpdate()
        {
            var patch = Shape as BSplinePatchGroup;
            if (patch == null)
                return;

            patch.SegmentsU = _segmentsU;
            patch.SegmentsV = _segmentsV;
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
            ((BSplinePatchGroup)Shape).IsPolygonRenderingEnabled = 
                IsPolygonRenderingEnabled;
        }

        private void SetupVirtualPointsGrid(
            int segmentsU, 
            int segmentsV, 
            double width = 1, 
            double height = 1
            )
        {
            var generated = 
                GridGenerator.Generate(segmentsU, segmentsV, width, height);

            SetControlPointsUsingPositions(generated);
        }

        private void SetupVirtualPointsCylinder(
            int lengthPoints, 
            int circlePoints, 
            double radius, 
            double height
            )
        {
            var generated = CylinderGenerator.Generate(
                lengthPoints,
                circlePoints,
                radius,
                height
                );

            SetControlPointsUsingPositions(generated);
        }

        private void SetControlPointsUsingPositions(
            IEnumerable<Point3D> generated
            )
        {
            _virtualPoints.Clear();
            _virtualPoints.AddRange(
                generated.Select(
                    t => new VirtualPoint()
                    {
                        Position = t,
                        ParentObject = this
                    }
                    ));
        }

        public override void Translate(Vector3D translation)
        {
            foreach (var virtualPoint in _virtualPoints)
                virtualPoint.Position += translation;
        }

        public void Save(SceneDataSaver saver)
        {
            var dataRowLength = 3 + _segmentsU;
            var totalRows = _virtualPoints.Count/dataRowLength;

            saver.EmitObjectInfo(Scene.WorldObjectType.BSplineSurface, Name);
            saver.EmitInt(dataRowLength);
            saver.EmitInt(totalRows);
            saver.EmitChar(_folded ? 'C' : 'R');
            saver.EmitChar('H');
            saver.EmitEndOfLine();

            for (var row = 0; row < totalRows; ++row)
            {
                for (var column = 0; column < dataRowLength; ++column)
                {
                    var dataRow = row;
                    var dataColumn = column;
                    var pos = _virtualPoints[dataColumn 
                        + dataRow * dataRowLength].SharedPoint;
                    saver.EmitInt(saver.CreateReferencePoint(pos));
                }
            }

            saver.EmitObjectDataEnd();
        }

        public void BuildFromExternalData(SharedPoint3D[,] data, bool folded)
        {
            var rows = data.GetLength(0);
            var rowLength = data.GetLength(1);
            _folded = folded;

            if (!folded)
            {
                _segmentsU = rowLength - 3;
                _segmentsV = rows - 3;
            }
            else
            {
                _segmentsU = rowLength - 3;
                _segmentsV = rows;
            }

            _virtualPoints.Clear();

            for (var y = 0; y < rows; y++)
            {
                for (var x = 0; x < rowLength; x++)
                {
                    _virtualPoints.Add(new VirtualPoint()
                    {
                        SharedPoint = data[y, x],
                        ParentObject = this,
                    });
                }
            }
        }

        public IParametricSurface GetParametricSurface()
        {
            var patch = (BSplinePatchGroup) Shape;
            return patch.GetParametricSurface();
        }
    }
}