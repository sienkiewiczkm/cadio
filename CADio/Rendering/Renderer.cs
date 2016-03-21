using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes;
using CADio.Mathematics;
using CADio.SceneManagement;

namespace CADio.Rendering
{
    public class Renderer : IRenderer
    {
        private static Color LeftEyeFilteredColor => Colors.Cyan;
        private static Color RightEyeFilteredColor => Colors.Red;

        private Scene _scene;

        public event EventHandler RenderOutputChanged;

        public double EyeDistance { get; set; } = 0.3;

        public Scene Scene
        {
            get { return _scene; }
            set
            {
                _scene = value;
                ForceRedraw();
            }
        }

        public void ForceRedraw()
        {
            RenderOutputChanged?.Invoke(this, null);
        }

        public IList<Line2D> GetRenderedPrimitives(PerspectiveType perspectiveType)
        {
            var perspectiveDistance = 2.0;

            Matrix4X4 projection;
            switch (perspectiveType)
            {
                case PerspectiveType.Standard:
                    projection = Transformations3D.SimplePerspective(perspectiveDistance);
                    break;
                case PerspectiveType.LeftEye:
                    projection = Transformations3D.SimplePerspectiveWithEyeShift(perspectiveDistance, -EyeDistance);
                    break;
                case PerspectiveType.RightEye:
                    projection = Transformations3D.SimplePerspectiveWithEyeShift(perspectiveDistance, +EyeDistance);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(perspectiveType), perspectiveType, null);
            }

            var worldMat = Scene.WorldTransformation;
            var transformation = projection*worldMat;

            var rasterizedLines = new List<Line2D>();

            var perspectiveColor = perspectiveType == PerspectiveType.LeftEye ? RightEyeFilteredColor : LeftEyeFilteredColor;

            foreach (var shape in Scene.Shapes)
            {
                for (var i = 0; i < shape.Indices.Count; ++i)
                {
                    var firstIndex = shape.Indices[i].First;
                    var secondIndex = shape.Indices[i].Second;

                    var firstPos = ((Vector3D) shape.Vertices[firstIndex].Position).ExtendTo4D().Transform(transformation);
                    var secondPos = ((Vector3D) shape.Vertices[secondIndex].Position).ExtendTo4D().Transform(transformation);

                    if (firstPos.W < 0 && secondPos.W < 0)
                        continue;

                    if (firstPos.W < 0 || secondPos.W < 0)
                    {
                        continue;
                    }

                    firstPos = firstPos.WDivide();
                    secondPos = secondPos.WDivide();

                    rasterizedLines.Add(new Line2D((Point) firstPos, (Point) secondPos, perspectiveColor));
                }
            }

            return rasterizedLines;
        }
    }
}
