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

namespace CADio.Rendering
{
    public class Renderer : IRenderer
    {
        public IList<Line2D> GetRenderedPrimitives()
        {
            var projection = Transformations3D.SimplePerspective(0.1)
                .Multiply(Transformations3D.Translation(new Vector3D(0, 0, 2)))
                .Multiply(Transformations3D.RotationX(Math.PI/3))
                .Multiply(Transformations3D.RotationY(Math.PI/3));
            var box = new Box {Size = new Vector3D(1, 1, 1)};

            var transformedVertices = box.Vertices
                .Select(t => ((Vector3D) t.Position).ExtendAffine().Transform(projection)).ToList();

            return box.Indices
                .Select(indicePair => new Line2D(
                    (Point) transformedVertices[indicePair.First], 
                    (Point) transformedVertices[indicePair.Second], 
                    Colors.Green
                )).ToList();
        }
    }
}
