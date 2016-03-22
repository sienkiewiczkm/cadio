using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using CADio.Views.ShapeEditors;

namespace CADio.Geometry.Shapes
{
    public class Torus : IShape, INotifyPropertyChanged
    {
        private double _smallRingRadius;
        private double _largeRingRadius;
        private int _smallRingSegmentsCount;
        private int _largeRingSegmentsCount;
        private IList<Vertex> _verticesCache;
        private IList<IndexedSegment> _indexedSegmentsCache;

        public Torus(int smallRingSegments = 32, int largeRingSegments = 32, 
            double smallRingRadius = 0.2, double largeRingRadius = 0.5)
        {
            _smallRingSegmentsCount = smallRingSegments;
            _largeRingSegmentsCount = largeRingSegments;
            _smallRingRadius = smallRingRadius;
            _largeRingRadius = largeRingRadius;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public double SmallRingRadius
        {
            get { return _smallRingRadius; }
            set
            {
                _smallRingRadius = value;
                RequestRecalculation();
                OnPropertyChanged();
            }
        }

        public double LargeRingRadius
        {
            get { return _largeRingRadius; }
            set
            {
                _largeRingRadius = value;
                RequestRecalculation();
                OnPropertyChanged();
            }
        }

        public int SmallRingSegmentsCount
        {
            get { return _smallRingSegmentsCount; }
            set
            {
                var isPropertyChanged = _smallRingSegmentsCount != value;
                _smallRingSegmentsCount = value;

                if (isPropertyChanged)
                {
                    RequestRecalculation();
                    OnPropertyChanged();
                }
            }
        }

        public int LargeRingSegmentsCount
        {
            get { return _largeRingSegmentsCount; }
            set
            {
                var isPropertyChanged = _largeRingSegmentsCount != value;
                _largeRingSegmentsCount = value;

                if (isPropertyChanged)
                {
                    RequestRecalculation();
                    OnPropertyChanged();
                }
            }
        }

        public string Name => "Torus";
        public bool IsEditable { get; set; } = true;

        public IList<Vertex> Vertices
        {
            get
            {
                if (_verticesCache == null)
                    RecalculateTorus();
                return _verticesCache;
            }
        }

        public IList<IndexedSegment> Segments
        {
            get
            {
                if (_indexedSegmentsCache == null)
                    RecalculateTorus();
                return _indexedSegmentsCache;
            }
        }

        public IList<Vertex> MarkerPoints => new List<Vertex>();

        public Control GetEditorControl()
        {
            return new TorusEditor()
            {
                DataContext = this
            };
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
            for (var i = 0; i < LargeRingSegmentsCount; ++i)
            {
                var phi = 2*Math.PI*i/LargeRingSegmentsCount;
                var sinphi = Math.Sin(phi);
                var cosphi = Math.Cos(phi);

                for (var j = 0; j < SmallRingSegmentsCount; ++j)
                {
                    var theta = 2*Math.PI*j/SmallRingSegmentsCount;
                    var sintheta = Math.Sin(theta);
                    var costheta = Math.Cos(theta);

                    var x = cosphi*(SmallRingRadius*sintheta + LargeRingRadius);
                    var y = SmallRingRadius*costheta;
                    var z = sinphi*(SmallRingRadius*sintheta + LargeRingRadius);

                    _verticesCache.Add(new Vertex(new Point3D(x, y, z)));
                }
            }

            // Create small rings
            for (var i = 0; i < LargeRingSegmentsCount; ++i)
            {
                var baseIndex = i*SmallRingSegmentsCount;
                for (var j = 0; j < SmallRingSegmentsCount; ++j)
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}