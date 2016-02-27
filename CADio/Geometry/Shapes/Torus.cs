using System;
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

        public Torus() : this(32, 32, 0.2, 0.5)
        {
            
        }

        public Torus(int smallRingSegments, int largeRingSegments, double smallRingRadius, double largeRingRadius)
        {
            _smallRingSegmentsCount = smallRingSegments;
            _largeRingSegmentsCount = largeRingSegments;
            _smallRingRadius = smallRingRadius;
            _largeRingRadius = largeRingRadius;
        }

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
            //   phi - duży pierścień
            //   theta - mały pierścień

            _verticesCache = new List<Vertex>();
            _indexedSegmentsCache = new List<IndexedSegment>();

            // Generate rings vertices
            for (int i = 0; i < LargeRingSegmentsCount; ++i)
            {
                var phi = 2*Math.PI*i/LargeRingSegmentsCount;
                var sinphi = Math.Sin(phi);
                var cosphi = Math.Cos(phi);

                for (int j = 0; j < SmallRingSegmentsCount; ++j)
                {
                    var theta = 2*Math.PI*j/SmallRingSegmentsCount;
                    var sintheta = Math.Sin(theta);
                    var costheta = Math.Cos(theta);

                    var x = cosphi*(SmallRingRadius*sintheta + LargeRingRadius);
                    var y = SmallRingRadius*costheta;
                    var z = sinphi*(SmallRingRadius*sintheta + LargeRingRadius);

                    var vertex = new Vertex
                    {
                        Position = new Point3D(x, y, z)
                    };

                    _verticesCache.Add(vertex);
                }
            }

            // Create small rings
            for (int i = 0; i < LargeRingSegmentsCount; ++i)
            {
                var baseIndex = i*SmallRingSegmentsCount;
                for (int j = 0; j < SmallRingSegmentsCount; ++j)
                {
                    var nextSubindex = (j + 1)%SmallRingSegmentsCount;
                    _indexedSegmentsCache.Add(new IndexedSegment(baseIndex+j, baseIndex+nextSubindex));
                }   
            }

            // Create large rings
            for (int i = 0; i < SmallRingSegmentsCount; ++i)
            {
                for (int j = 0; j < LargeRingSegmentsCount; ++j)
                {
                    var nextj = (j + 1)%LargeRingSegmentsCount;
                    var firstIndex = j*SmallRingSegmentsCount + i;
                    var secondIndex = nextj*SmallRingSegmentsCount + i;
                    _indexedSegmentsCache.Add(new IndexedSegment(firstIndex, secondIndex));
                }
            }
        }
    }
}