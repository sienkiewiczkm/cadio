﻿using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes.Dynamic;

namespace CADio.SceneManagement
{
    public class BezierSurfaceWorldObject : WorldObject
    {
        private readonly int _segmentsX;
        private readonly int _segmentsY;
        private bool _isPolygonRenderingEnabled;
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

        public BezierSurfaceWorldObject(int segmentsX, int segmentsY)
        {
            _segmentsX = segmentsX;
            _segmentsY = segmentsY;
            SetupVirtualPointsGrid(3*segmentsX+1, 3*segmentsY+1);
        }

        public override void PrerenderUpdate()
        {
            var patch = Shape as BezierPatchGroup;
            if (patch == null)
                return;

            patch.SegmentsX = _segmentsX;
            patch.SegmentsY = _segmentsY;
            patch.ControlPoints = _virtualPoints.Select(t => t.Position).ToList();
        }

        public override ICollection<ISceneSelectable> GetSelectableChildren()
        {
            return _virtualPoints.Cast<ISceneSelectable>().ToList();
        }

        private void SetupPolygonRendering()
        {
            if (!(Shape is BezierPatch))
                return;
            ((BezierPatch)Shape).IsPolygonRenderingEnabled = IsPolygonRenderingEnabled;
        }

        private void SetupVirtualPointsGrid(int cols, int rows)
        {
            const double spacingX = 1.0;
            const double spacingY = 1.0;
            var totalX = spacingX*(cols-1);
            var totalY = spacingY*(rows-1);

            _virtualPoints.Clear();

            for (var y = 0; y < rows; y++)
            {
                for (var x = 0; x < cols; x++)
                {
                    _virtualPoints.Add(new VirtualPoint()
                    {
                        Position = new Point3D(x*spacingX - totalX*0.5, 0, y*spacingY - totalY*0.5)
                    });
                }
            }
        }
    }
}