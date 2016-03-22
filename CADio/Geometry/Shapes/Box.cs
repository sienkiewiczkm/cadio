using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace CADio.Geometry.Shapes
{
    public class Box : IShape
    {
        private Vector3D _size;
        private IList<Vertex> _verticesCache;
        private IList<IndexedSegment> _indexedSegmentsCache;

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
        public bool IsEditable { get; set; } = true;

        public IList<Vertex> Vertices
        {
            get
            {
                if (_verticesCache == null)
                    RecalculateMesh();
                return _verticesCache;
            }
        }

        public IList<IndexedSegment> Segments
        {
            get
            {
                if (_indexedSegmentsCache == null)
                    RecalculateMesh();
                return _indexedSegmentsCache;
            }
        }

        public IList<Vertex> MarkerPoints => new List<Vertex>(); 

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

            _indexedSegmentsCache = new List<IndexedSegment>
            {
                new IndexedSegment(0, 1),
                new IndexedSegment(1, 2),
                new IndexedSegment(2, 3),
                new IndexedSegment(3, 0),

                new IndexedSegment(4, 5),
                new IndexedSegment(5, 6),
                new IndexedSegment(6, 7),
                new IndexedSegment(7, 4),

                new IndexedSegment(0, 4),
                new IndexedSegment(1, 5),
                new IndexedSegment(2, 6),
                new IndexedSegment(3, 7),
            };
        }
    }
}