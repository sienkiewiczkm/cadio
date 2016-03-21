using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CADio.Geometry.Shapes
{
    public class Cursor3D : IShape
    {
        private double _size = 1.0;
        private IList<Vertex> _verticesCache;
        private IList<IndexedSegment> _indexedSegmentsCache;

        public string Name => "Cursor 3D";
        public bool IsEditable { get; set; } = true;

        public double Size
        {
            get { return _size; }
            set
            {
                _size = value;
                RequestRecalculation();
            }
        }

        public IList<Vertex> Vertices
        {
            get
            {
                if (_verticesCache == null)
                    RecalculateTopology();
                return _verticesCache;
            }
        }

        public IList<IndexedSegment> Segments
        {
            get
            {
                if (_indexedSegmentsCache == null)
                    RecalculateTopology();
                return _indexedSegmentsCache;
            }
        }

        public Control GetEditorControl()
        {
            return null;
        }

        private void RequestRecalculation()
        {
            _verticesCache = null;
            _indexedSegmentsCache = null;
        }

        private void RecalculateTopology()
        {
            _verticesCache = new List<Vertex>
            {
                new Vertex(new Point3D(0, +1, 0), Colors.Red),
                new Vertex(new Point3D(0, -1, 0), Colors.Red),
                new Vertex(new Point3D(+1, 0, 0), Colors.Green),
                new Vertex(new Point3D(-1, 0, 0), Colors.Green),
                new Vertex(new Point3D(0, 0, +1), Colors.Blue),
                new Vertex(new Point3D(0, 0, -1), Colors.Blue),
            };

            _indexedSegmentsCache = new List<IndexedSegment>
            {
                new IndexedSegment(0, 1),
                new IndexedSegment(2, 3),
                new IndexedSegment(4, 5),
            };
        }
    }
}