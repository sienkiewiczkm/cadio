using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.Mathematics;

namespace CADio.Geometry.Shapes.Builders
{
    public class WireframeBuilder
    {
        private List<Vertex> _vertices = new List<Vertex>();
        private List<IndexedLine> _indices = new List<IndexedLine>();
        private int? _previousIndex;

        public IReadOnlyList<Vertex> Vertices => _vertices.AsReadOnly();
        public IReadOnlyList<IndexedLine> Lines => _indices.AsReadOnly();

        public void DrawVector(
            Point3D point, 
            Vector3D vector, 
            double length = 0.5,
            Color? color = null
            )
        {
            if (MathHelpers.AlmostEqual(vector.Length, 0, double.Epsilon))
                return;

            vector.Normalize();
            var target = point + length*vector;
            var vectorColor = color ?? Colors.Red;

            FinishChain();
            Connect(point, vectorColor);
            Connect(target, vectorColor);
            FinishChain();
        }

        public void Connect(Point3D nextPoint, Color? color = null)
        {
            var currentIndex = _vertices.Count;
            _vertices.Add(new Vertex(nextPoint, color ?? Colors.White));
            if (_previousIndex.HasValue)
                _indices.Add(
                    new IndexedLine(_previousIndex.Value, currentIndex)
                );
            _previousIndex = currentIndex;
        }

        public void FinishChain()
        {
            _previousIndex = null;
        }

    }
}