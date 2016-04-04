using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace CADio.Geometry.Shapes.Static
{
    public class Sphere : IShape
    {
        private double _radius = 1.0;
        private int _horizontalCrossSections = 8;
        private int _crossSectionVertices = 8;
        private IList<Vertex> _verticesCache;
        private IList<IndexedLine> _indexedSegmentsCache;

        public int HorizontalCrossSections
        {
            get { return _horizontalCrossSections; }
            set
            {
                _horizontalCrossSections = Math.Max(3, value);
                RequestRecalculation();
            }
        }

        public int CrossSectionVertices
        {
            get { return _crossSectionVertices; }
            set
            {
                _crossSectionVertices = Math.Max(3, value);
                RequestRecalculation();
            }
        }

        public double Radius
        {
            get { return _radius; }
            set
            {
                RequestRecalculation();
                _radius = value;
            }
        }

        public string Name => "Sphere";

        public IList<Vertex> Vertices
        {
            get
            {
                if (_verticesCache == null)
                    RecalculateMesh();
                return _verticesCache;
            }
        }

        public IList<IndexedLine> Lines
        {
            get
            {
                if (_indexedSegmentsCache == null)
                    RecalculateMesh();
                return _indexedSegmentsCache;
            }
        }

        public IList<Vertex> MarkerPoints => new List<Vertex>();
        public IList<Vertex> RawPoints => new List<Vertex>();

        public Control GetEditorControl()
        {
            return null;
        }

        private void RequestRecalculation()
        {
            _verticesCache = null;
            _indexedSegmentsCache = null;
        }

        private void RecalculateMesh()
        {
            _verticesCache = new List<Vertex>(2 + (HorizontalCrossSections - 2)*CrossSectionVertices);
            _indexedSegmentsCache = new List<IndexedLine>();

            _verticesCache.Add(new Vertex(new Point3D(0, +Radius, 0)));
            _verticesCache.Add(new Vertex(new Point3D(0, -Radius, 0)));

            for (var i = 1; i < HorizontalCrossSections - 1; ++i)
            {
                var omega = Math.PI*i/(HorizontalCrossSections - 1);
                var sinomega = Math.Sin(omega);
                var cosomega = Math.Cos(omega);

                for (var j = 0; j < CrossSectionVertices; ++j)
                {
                    var phi = 2*Math.PI*j/CrossSectionVertices;
                    var sinphi = Math.Sin(phi);
                    var cosphi = Math.Cos(phi);

                    var x = Radius*cosphi*sinomega;
                    var y = Radius*cosomega;
                    var z = Radius*sinphi*sinomega;

                    _verticesCache.Add(new Vertex(new Point3D(x, y, z)));
                }
            }

            for (var i = 1; i < HorizontalCrossSections - 1; ++i)
            {
                var baseIndex = 2 + (i - 1)*CrossSectionVertices;
                for (var j = 0; j < CrossSectionVertices; ++j)
                {
                    var nextSubindex = (j + 1)%CrossSectionVertices;
                    _indexedSegmentsCache.Add(
                        new IndexedLine(baseIndex + j, baseIndex + nextSubindex)
                    );
                }
            }

            for (var i = 0; i < CrossSectionVertices; ++i)
            {
                _indexedSegmentsCache.Add(new IndexedLine(0, 2 + i));
                _indexedSegmentsCache.Add(new IndexedLine(1, _verticesCache.Count - i - 1));
            }

            for (var i = 1; i < HorizontalCrossSections - 2; ++i)
            {
                var currentBaseIndex = 2 + (i - 1)*CrossSectionVertices;
                var lowerBaseIndex = 2 + i*CrossSectionVertices;

                for (var j = 0; j < CrossSectionVertices; ++j)
                {
                    _indexedSegmentsCache.Add(
                        new IndexedLine(currentBaseIndex + j, lowerBaseIndex + j)
                    );
                }
            }
        }
    }
}