using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes;
using CADio.Geometry.Shapes.Dynamic;
using CADio.Mathematics;
using CADio.SceneManagement.Interfaces;
using CADio.SceneManagement.Surfaces;

namespace CADio.SceneManagement
{
    public class GregoryPatchWorldObject : WorldObject
    {
        public GregoryPatchShape GregoryPatchShape { get; private set; }

        public GregoryPatchWorldObject()
        {
            GregoryPatchShape = new GregoryPatchShape();
            Shape = GregoryPatchShape;
        }
    }
}