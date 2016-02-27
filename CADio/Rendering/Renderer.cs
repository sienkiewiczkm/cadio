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
        public event EventHandler RenderOutputChanged;

        public double XRotation { get; set; }
        public double YRotation { get; set; }

        public void ForceRedraw()
        {
            RenderOutputChanged?.Invoke(this, null);
        }

        public IList<Line2D> GetRenderedPrimitives()
        {
            var projection = Transformations3D.SimplePerspective(2);
            var worldMat = Transformations3D.Translation(new Vector3D(0, 0, 1)) * Transformations3D.RotationX(XRotation) 
                * Transformations3D.RotationY(YRotation);
            var box = new Box {Size = new Vector3D(1, 1, 1)};

            var transformation = projection*worldMat;
            var transformedVertices = box.Vertices
                .Select(t => ((Vector3D) t.Position).ExtendTo4D().Transform(transformation).WDivide()).ToList();

            return box.Indices
                .Select(indicePair => new Line2D(
                    (Point) transformedVertices[indicePair.First], 
                    (Point) transformedVertices[indicePair.Second], 
                    Colors.Green
                )).ToList();
        }
    }
}
