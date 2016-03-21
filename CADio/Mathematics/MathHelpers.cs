using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CADio.Mathematics
{
    public static class MathHelpers
    {
        public static byte Lerp(byte a, byte b, double t)
        {
            return (byte) (a + (b - a)*t);
        }

        public static double Clamp(double val, double min, double max)
        {
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }
    }
}
