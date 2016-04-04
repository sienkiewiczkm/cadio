using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CADio.Geometry.Shapes.Static
{
    public class Cursor3D : IShape
    {
        private double _size = 1.0;
        private IList<Vertex> _verticesCache;
        private IList<IndexedLine> _indexedSegmentsCache;

        public string Name => "Cursor 3D";

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

        public IList<IndexedLine> Lines
        {
            get
            {
                if (_indexedSegmentsCache == null)
                    RecalculateTopology();
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

            _indexedSegmentsCache = new List<IndexedLine>
            {
                new IndexedLine(0, 1),
                new IndexedLine(2, 3),
                new IndexedLine(4, 5),
            };
        }
    }
}