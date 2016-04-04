using System.Collections.Generic;
using System.Windows.Controls;

namespace CADio.Geometry.Shapes
{
    public interface IShape
    {
        /// <summary>
        /// Shape readable name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Vertex buffer. Feed for Lines.
        /// </summary>
        IList<Vertex> Vertices { get; }

        /// <summary>
        /// Line index buffer. Indices corresponds to Vertices list.
        /// </summary>
        IList<IndexedLine> Lines { get; }

        /// <summary>
        /// Marker points. They are rendered as thick points (constant size).
        /// </summary>
        IList<Vertex> MarkerPoints { get; }

        /// <summary>
        /// Raw points in space rendered as small as possible (pixel size).
        /// </summary>
        IList<Vertex> RawPoints { get; }

        Control GetEditorControl();
    }
}