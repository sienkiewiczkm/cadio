using System.Collections.Generic;
using System.Windows.Controls;

namespace CADio.Geometry.Shapes
{
    public interface IShape
    {
        string Name { get; }
        IList<Vertex> Vertices { get; }
        IList<IndexedSegment> Indices { get; }

        Control GetEditorControl();
    }
}