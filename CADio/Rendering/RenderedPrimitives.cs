using System.Collections.Generic;
using CADio.Geometry;

namespace CADio.Rendering
{
    public class RenderedPrimitives
    {
        public IList<Vertex2D> Points { get; set; } = new List<Vertex2D>();
        public IList<Line2D> Lines { get; set; } = new List<Line2D>();
    }
}