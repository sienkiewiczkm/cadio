using System.Windows.Media;
using CADio.Mathematics;

namespace CADio.Helpers
{
    public static class ColorHelpers
    {
        public static Color Lerp(Color a, Color b, double t)
        {
            return Color.FromRgb(
                MathHelpers.Lerp(a.R, b.R, t),
                MathHelpers.Lerp(a.G, b.G, t),
                MathHelpers.Lerp(a.B, b.B, t)
            );
        }
    }
}