using System;
using System.Collections.Generic;

namespace CADio.Rendering
{
    public interface IRenderer
    {
        event EventHandler RenderOutputChanged;

        RenderedPrimitives GetRenderedPrimitives(PerspectiveType perspectiveType, RenderTarget renderTarget);
    }
}