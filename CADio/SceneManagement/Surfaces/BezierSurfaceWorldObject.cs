using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.Configuration;
using CADio.Geometry.Generators;
using CADio.Geometry.Shapes.Dynamic;
using CADio.Helpers.MVVM;
using CADio.Mathematics.Interfaces;
using CADio.Mathematics.Patches;
using CADio.Mathematics.Surfaces;
using CADio.SceneManagement.Interfaces;
using CADio.SceneManagement.Points;
using CADio.SceneManagement.Serialization;

namespace CADio.SceneManagement.Surfaces
{
    public class BezierSurfaceWorldObject : 
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

        protected int RowLength => 3*_segmentsU + 1;

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
                    (_togglePolygonRenderingCommand = new RelayCommand(
                        TogglePolygonRendering
                    ));
            }
        }

        private void TogglePolygonRendering()
        {
            IsPolygonRenderingEnabled = !IsPolygonRenderingEnabled;
        }

        public BezierSurfaceWorldObject()
        {
            Shape = new BezierPatchGroup();
        }

        public static BezierSurfaceWorldObject CreateFlatGrid(
            int segmentsU, 
            int segmentsV, 
            double width, 
            double height
            )
        {
            var surface = new BezierSurfaceWorldObject
            {
                _segmentsU = segmentsU,
                _segmentsV = segmentsV,
            };

            surface.SetupVirtualPointsGrid(
                3*segmentsU+1, 
                3*segmentsV+1, 
                width, 
                height
            );

            return surface;
        }

        public static BezierSurfaceWorldObject CreateCylindrical(
            int segmentsU, 
            int segmentsV,
            double radius, 
            double height
            )
        {
            var surface = new BezierSurfaceWorldObject
            {
                _segmentsU = segmentsU,
                _segmentsV = segmentsV,
                _folded = true,
            };

            surface.SetupVirtualPointsCylinder(
                3*segmentsU+1, 
                3*segmentsV, 
                radius, 
                height
            );

            return surface;
        }

        public override void PrerenderUpdate()
        {
            var patch = Shape as BezierPatchGroup;
            if (patch == null)
                return;

            patch.SegmentsU = _segmentsU;
            patch.SegmentsV = _segmentsV;
            patch.ControlPoints = _virtualPoints
                .Select(t => new SurfaceControlPoint() {
                    Position = t.Position,
                    ColorOverride = t.IsGrabbed
                        ? ColorSettings.SceneSelection
                        : (Color?) null,
                }).ToList();
        }

        public override ICollection<ISceneSelectable> GetSelectableChildren()
        {
            return _virtualPoints.Cast<ISceneSelectable>().ToList();
        }

        private void SetupPolygonRendering()
        {
            var bezierPatchGroup = Shape as BezierPatchGroup;
            if (bezierPatchGroup != null)
                bezierPatchGroup.IsPolygonRenderingEnabled = 
                    IsPolygonRenderingEnabled;
        }

        public override void Translate(Vector3D translation)
        {
            foreach (var virtualPoint in _virtualPoints)
                virtualPoint.Position += translation;
        }

        public Tuple<int, int> GetVirtualPointCoordinate(VirtualPoint vp)
        {
            var realVirtualPoint = vp.GetAdjacentVirtualPointFor(this);
            Debug.Assert(realVirtualPoint != null);

            var index = _virtualPoints.IndexOf(realVirtualPoint);
            var rowLength = 3*_segmentsU + 1;
            var x = index%rowLength;
            var y = index/rowLength;
            return new Tuple<int, int>(x, y);
        }

        public VirtualPoint GetVirtualPoint(int x, int y)
        {
            var rowLength = 3*_segmentsU + 1;
            return _virtualPoints[y*rowLength + x];
        }

        public BernsteinPatch GetBernsteinPatch()
        {
            if (_segmentsU > 1 || _segmentsV > 1)
                throw new ArgumentException(
                    "There exists more than one bernstein patch here"
                );

            var bezierPatch = new BernsteinPatch();
            for (var i = 0; i < 16; ++i)
                bezierPatch.ControlPoints[i / 4, i % 4] = 
                    _virtualPoints[i].Position;
            return bezierPatch;
        }

        public IParametricSurface GetParametricSurface()
        {
            return ((BezierPatchGroup) Shape).GetParametricSurface();
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

        public void Save(SceneDataSaver saver)
        {
            var totalRows = _virtualPoints.Count / RowLength;

            saver.EmitObjectInfo(Scene.WorldObjectType.BezierSurface, Name);
            saver.EmitInt(RowLength);
            saver.EmitInt(totalRows);
            saver.EmitChar(_folded ? 'C' : 'R');
            saver.EmitChar('H');
            saver.EmitEndOfLine();

            for (var row = 0; row < totalRows; ++row)
            {
                for (var column = 0; column < RowLength; ++column)
                {
                    var dataRow = row;
                    var dataColumn = column;
                    var pos = _virtualPoints[dataColumn + 
                        dataRow*RowLength].SharedPoint;
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
                _segmentsU = (rowLength - 1) / 3;
                _segmentsV = (rows - 1)/3;
            }
            else
            {
                _segmentsU = (rowLength - 1)/3;
                _segmentsV = rows/3;
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

        public static List<BezierSurfaceWorldObject> GetAdjacentSurfaces(
            VirtualPoint virtualPoint
            )
        {
            return virtualPoint.GetAdjacentObjects()
                .Where(t => t is BezierSurfaceWorldObject)
                .Cast<BezierSurfaceWorldObject>()
                .ToList();
        }

        public static List<VirtualPoint> FindHoleOutline(
            List<BezierSurfaceWorldObject> gregoryAdjacentPatches
            )
        {
            var outline = new List<VirtualPoint>();
            BezierSurfaceWorldObject lastProcessedPatch = null;
            var nonProcessedPatches = new LinkedList<BezierSurfaceWorldObject>(
                gregoryAdjacentPatches
            );

            var currentPatch = nonProcessedPatches.First();
            while (nonProcessedPatches.Count > 0)
            {
                nonProcessedPatches.Remove(currentPatch);

                var importantCollapsedPoints = currentPatch._virtualPoints
                    .Where(t => gregoryAdjacentPatches.Intersect(
                            GetAdjacentSurfaces(t)
                        ).Count() >= 2
                    )
                    .ToList();

                if (importantCollapsedPoints.Count != 2)
                    return new List<VirtualPoint>();
                    /*
                    throw new ApplicationException(string.Join(
                        "Expected two shared points between adjacent patches,",
                        $" but found ${importantCollapsedPoints.Count}."
                    ));*/

                if (lastProcessedPatch != null)
                {
                    var firstPoint = importantCollapsedPoints.First();
                    if (!GetAdjacentSurfaces(firstPoint)
                        .Contains(lastProcessedPatch))
                        importantCollapsedPoints.Reverse();
                }

                if (lastProcessedPatch == null)
                    outline.Add(importantCollapsedPoints[0]);

                var a = currentPatch.GetVirtualPointCoordinate(
                    importantCollapsedPoints[0]
                );

                var b = currentPatch.GetVirtualPointCoordinate(
                    importantCollapsedPoints[1]
                );

                for (var i = 1; i < 3; ++i)
                {
                    var xcoord = (a.Item1*(3 - i) + b.Item1*i)/3;
                    var ycoord = (a.Item2*(3 - i) + b.Item2*i)/3;
                    outline.Add(currentPatch.GetVirtualPoint(xcoord, ycoord));
                }

                if (nonProcessedPatches.Count > 0)
                    outline.Add(importantCollapsedPoints[1]);

                lastProcessedPatch = currentPatch;
                currentPatch = GetAdjacentSurfaces(
                    importantCollapsedPoints[1]
                ).First(t => t != lastProcessedPatch);
            }

            return outline;
        }
    }
}