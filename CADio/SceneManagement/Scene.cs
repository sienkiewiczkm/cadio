using System.Collections.Generic;
using CADio.Geometry.Shapes;
using CADio.Mathematics;

namespace CADio.SceneManagement
{
    public class Scene
    {
        public Matrix4x4 WorldTransformation { get; set; } = Matrix4x4.Identity;
        public List<IShape> Shapes { get; set; } = new List<IShape>();
    }
}