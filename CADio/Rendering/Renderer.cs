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
                for (var i = 0; i < shape.Indices.Count; ++i)
                {
                    var firstIndex = shape.Indices[i].First;
                    var secondIndex = shape.Indices[i].Second;

                    var firstPos = ((Vector3D) shape.Vertices[firstIndex].Position).ExtendTo4D().Transform(transformation);
                    var secondPos = ((Vector3D) shape.Vertices[secondIndex].Position).ExtendTo4D().Transform(transformation);

                    if (firstPos.W < 0 && secondPos.W < 0)
                        continue;

                    // clip
                    if (firstPos.W < 0 || secondPos.W < 0)
                    {
                        continue;
                        /*if (firstPos.W < 0)
                        {
                            var temp = firstPos;
                            firstPos = secondPos;
                            secondPos = temp;
                        }

                        var x = (secondPos.W / (secondPos.W - firstPos.W)) * (firstPos.X - secondPos.X) + secondPos.X;
                        var y = (secondPos.W / (secondPos.W - firstPos.W)) * (firstPos.Y - secondPos.Y) + secondPos.Y;

                        secondPos = new Vector4D(x, y, 0, 1);*/
                    }

                    firstPos = firstPos.WDivide();
                    secondPos = secondPos.WDivide();

                    rasterizedLines.Add(new Line2D(
                        (Point) firstPos,
                        (Point) secondPos,
                        Colors.Green)
                    );
                }

                /*
                var transformedVertices = shape.Vertices
                    .Select(t => ((Vector3D) t.Position).ExtendTo4D().Transform(transformation).WDivide()).ToList();

                rasterizedLines.AddRange(
                    shape.Indices.Select(indicePair => new Line2D(
                        (Point) transformedVertices[indicePair.First],
                        (Point) transformedVertices[indicePair.Second],
                        Colors.Green
                    )).ToList()
                );
                */
            }

            return rasterizedLines;
        }
    }
}
