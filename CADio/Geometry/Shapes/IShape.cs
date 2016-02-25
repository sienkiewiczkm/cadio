using System.Collections.Generic;

namespace CADio.Geometry.Shapes
{
    public interface IShape
    {
        IList<Vertex> Vertices { get; }
        IList<IndexedSegment> Indices { get; }
    }
}