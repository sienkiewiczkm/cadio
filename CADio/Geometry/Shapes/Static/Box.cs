using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace CADio.Geometry.Shapes.Static
{
    public class Box : IShape
    {
        private Vector3D _size;
        private IList<Vertex> _verticesCache;
        private IList<IndexedLine> _indexedSegmentsCache;

        public Vector3D Size
        {
            get { return _size; }
            set
            {
                RequestRecalculation();
                _size = value;
            }
        }

        public string Name => "Box";

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
        public IList<Vertex> RawPoints { get; set; }

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
            var halfSize = Size*0.5;

            _verticesCache = new List<Vertex>
            {
                new Vertex(new Point3D(+halfSize.X, -halfSize.Y, +halfSize.Z)), // 0
                new Vertex(new Point3D(-halfSize.X, -halfSize.Y, +halfSize.Z)), // 1
                new Vertex(new Point3D(-halfSize.X, -halfSize.Y, -halfSize.Z)), // 2
                new Vertex(new Point3D(+halfSize.X, -halfSize.Y, -halfSize.Z)), // 3

                new Vertex(new Point3D(+halfSize.X, +halfSize.Y, +halfSize.Z)), // 4
                new Vertex(new Point3D(-halfSize.X, +halfSize.Y, +halfSize.Z)), // 5
                new Vertex(new Point3D(-halfSize.X, +halfSize.Y, -halfSize.Z)), // 6
                new Vertex(new Point3D(+halfSize.X, +halfSize.Y, -halfSize.Z)), // 7
            };

            _indexedSegmentsCache = new List<IndexedLine>
            {
                new IndexedLine(0, 1),
                new IndexedLine(1, 2),
                new IndexedLine(2, 3),
                new IndexedLine(3, 0),

                new IndexedLine(4, 5),
                new IndexedLine(5, 6),
                new IndexedLine(6, 7),
                new IndexedLine(7, 4),

                new IndexedLine(0, 4),
                new IndexedLine(1, 5),
                new IndexedLine(2, 6),
                new IndexedLine(3, 7),
            };
        }
    }
}