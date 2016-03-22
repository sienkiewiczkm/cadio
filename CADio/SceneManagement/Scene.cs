using System.Collections.Generic;
using CADio.Geometry.Shapes;
using CADio.Mathematics;
using CADio.Rendering;

namespace CADio.SceneManagement
{
    public class Scene
    {
        public Camera Camera { get; set; } = new Camera();
        public List<WorldObject> Objects { get; set; } = new List<WorldObject>();
    }
}