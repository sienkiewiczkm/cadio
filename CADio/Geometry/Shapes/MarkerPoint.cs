using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace CADio.Geometry.Shapes
{
    public class MarkerPoint : IShape
    {
        private List<Vertex> _markerPointsCache;
         
        public string Name { get; set; } = "Point";
        public bool IsEditable { get; set; } = true;

        public IList<Vertex> Vertices => new List<Vertex>();
        public IList<IndexedSegment> Segments => new List<IndexedSegment>();

        public IList<Vertex> MarkerPoints
        {
            get
            {
                if (_markerPointsCache == null)
                    RecalculateTopology();
                return _markerPointsCache;
            }
        }

        public Control GetEditorControl()
        {
            return null;
        }

        private void RecalculateTopology()
        {
            _markerPointsCache = new List<Vertex>
            {
                new Vertex(new Point3D(0, 0, 0)),
            };
        }
    }
}