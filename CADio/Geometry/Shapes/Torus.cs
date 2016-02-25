using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Media3D;

namespace CADio.Geometry.Shapes
{
    public class Torus : IShape
    {
        private double _smallRingRadius;
        private double _largeRingRadius;
        private int _smallRingSegmentsCount;
        private int _largeRingSegmentsCount;
        private IList<Vertex> _verticesCache;
        private IList<IndexedSegment> _indexedSegmentsCache;

        public double SmallRingRadius
        {
            get { return _smallRingRadius; }
            set
            {
                RequestRecalculation();
                _smallRingRadius = value;
            }
        }

        public double LargeRingRadius
        {
            get { return _largeRingRadius; }
            set
            {
                RequestRecalculation();
                _largeRingRadius = value;
            }
        }

        public int SmallRingSegmentsCount
        {
            get { return _smallRingSegmentsCount; }
            set
            {
                if (_smallRingSegmentsCount != value)
                    RequestRecalculation();
                _smallRingSegmentsCount = value;
            }
        }

        public int LargeRingSegmentsCount
        {
            get { return _largeRingSegmentsCount; }
            set
            {
                if (_largeRingSegmentsCount != value)
                    RequestRecalculation();
                _largeRingSegmentsCount = value;
            }
        }

        public IList<Vertex> Vertices
        {
            get
            {
                if (_verticesCache == null)
                    RecalculateTorus();
                return _verticesCache;
            }
        }

        public IList<IndexedSegment> Indices
        {
            get
            {
                if (_indexedSegmentsCache == null)
                    RecalculateTorus();
                return _indexedSegmentsCache;
            }
        }

        private void RequestRecalculation()
        {
            _verticesCache = null;
            _indexedSegmentsCache = null;
        }

        private void RecalculateTorus()
        {
            // x = cos(phi) (r sin(theta) + R)
            // y = r cos(theta)
            // z = sin(phi) (r sin(theta) + R)
            //   phi, theta in [0, 2pi)

            _verticesCache = new List<Vertex>()
            {
                new Vertex() { Position = new Point3D(0, 0, 0) },
                new Vertex() { Position = new Point3D(10, 10, 0) },
            };

            _indexedSegmentsCache = new List<IndexedSegment>()
            {
                new IndexedSegment(0, 1)
            };
        }
    }
}