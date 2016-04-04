using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace CADio.Mathematics
{
    public static class MathHelpers
    {
        public static byte Lerp(byte a, byte b, double t)
        {
            return (byte) (a + (b - a)*t);
        }

        public static double Lerp(double a, double b, double t)
        {
            return a + (b - a) * t;
        }

        public static Point3D Lerp(Point3D a, Point3D b, double t)
        {
            return new Point3D(
                Lerp(a.X, b.X, t),
                Lerp(a.Y, b.Y, t),
                Lerp(a.Z, b.Z, t)
            );
        }

        public static double Clamp(double val, double min, double max)
        {
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }

        public static Point3D MakePoint3D(double[] array)
        {
            if (array.Length != 3)
                throw new ArgumentException("Point3D requires exactly 3 coordinates.");
            return new Point3D(array[0], array[1], array[2]);
        }
    }
}
