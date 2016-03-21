using System.Collections.Generic;
using CADio.Geometry.Shapes;
using CADio.Mathematics;
using CADio.Rendering;

namespace CADio.SceneManagement
{
    public class Scene
    {
        public Camera Camera { get; set; } = new Camera();
        public Matrix4X4 WorldTransformation { get; set; } = Matrix4X4.Identity;
        public List<IShape> Shapes { get; set; } = new List<IShape>();
    }
}