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
        private const double MinWorldScale = 0.05;

        private Scene _scene;

        public event EventHandler RenderOutputChanged;

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

        public IList<Line2D> GetRenderedPrimitives()
        {
            var projection = Transformations3D.SimplePerspective(2);
            var worldMat = Scene.WorldTransformation;

            var transformation = projection*worldMat;

            var rasterizedLines = new List<Line2D>();

            foreach (var shape in Scene.Shapes)
            {
                var transformedVertices = shape.Vertices
                    .Select(t => ((Vector3D) t.Position).ExtendTo4D().Transform(transformation).WDivide()).ToList();

                rasterizedLines.AddRange(
                    shape.Indices.Select(indicePair => new Line2D(
                        (Point) transformedVertices[indicePair.First],
                        (Point) transformedVertices[indicePair.Second],
                        Colors.Green
                    )).ToList()
                );
            }

            return rasterizedLines;
        }
    }
}
