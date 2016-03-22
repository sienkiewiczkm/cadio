using System.Windows;
using System.Windows.Media;

namespace CADio.Rendering
{
    public struct Vertex2D
    {
        public Point Position;
        public Color Color;

        public Vertex2D(Point position, Color color)
        {
            Position = position;
            Color = color;
        }
    }
}