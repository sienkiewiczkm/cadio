using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes;
using CADio.Geometry.Shapes.Dynamic;
using CADio.Mathematics;
using CADio.SceneManagement.Interfaces;

namespace CADio.SceneManagement
{
    public class GregoryPatchWorldObject : WorldObject
    {
        public GregoryPatchWorldObject()
        {
            Shape = new GregoryPatchShape();
        }
    }
}