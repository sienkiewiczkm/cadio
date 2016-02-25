using System.Windows;
using System.Windows.Media;

namespace CADio.Rendering
{
    public struct Line2D
    {
        public Point From;
        public Point To;
        public Color Color;

        public Line2D(Point from, Point to, Color color)
        {
            From = from;
            To = to;
            Color = color;
        }
    }
}