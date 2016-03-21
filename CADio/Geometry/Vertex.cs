using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CADio.Geometry
{
    public struct Vertex
    {
        public static Color DefaultColor = Colors.White;

        public Point3D Position;
        public Color Color;

        public Vertex(Point3D position)
        {
            Position = position;
            Color = DefaultColor;
        }

        public Vertex(Point3D position, Color color)
        {
            Position = position;
            Color = color;
        }
    }
}