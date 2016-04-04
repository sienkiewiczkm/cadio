using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace CADio.Geometry.Shapes.Static
{
    public class MarkerPoint : IShape
    {
        private List<Vertex> _markerPointsCache;
         
        public string Name { get; set; } = "Point";

        public IList<Vertex> Vertices => new List<Vertex>();
        public IList<IndexedLine> Lines => new List<IndexedLine>();
        public IList<Vertex> RawPoints => new List<Vertex>();

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