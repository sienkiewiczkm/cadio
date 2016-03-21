using System.Collections.Generic;
using System.Windows.Controls;

namespace CADio.Geometry.Shapes
{
    public interface IShape
    {
        string Name { get; }
        bool IsEditable { get; set; }
        IList<Vertex> Vertices { get; }
        IList<IndexedSegment> Segments { get; }

        Control GetEditorControl();
    }
}