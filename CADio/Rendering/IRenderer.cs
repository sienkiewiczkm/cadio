using System.Collections.Generic;

namespace CADio.Rendering
{
    public interface IRenderer
    {
        IList<Line2D> GetRenderedPrimitives();
    }
}