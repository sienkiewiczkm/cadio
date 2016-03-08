using System;
using System.Windows;
using System.Windows.Media.Media3D;

namespace CADio.Mathematics
{
    public struct Vector4D
    {
        public double X;
        public double Y;
        public double Z;
        public double W;

        public Vector4D(double x, double y, double z, double w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public double LengthPow2 => X*X + Y*Y + Z*Z + W*W;
        public double Length => Math.Sqrt(LengthPow2);

        public double[] ToArray()
        {
            return new [] { X, Y, Z, W };
        }

        public Vector4D Transform(Matrix4X4 transformation)
        {
            var vector = ToArray();
            var output = new double[4];
            for (int row = 0; row < 4; ++row)
                for (int j = 0; j < 4; ++j)
                    output[row] += transformation[row, j]*vector[j];
            return new Vector4D {X = output[0], Y = output[1], Z = output[2], W = output[3]};
        }

        public Vector4D WDivide()
        {
            return new Vector4D(X / W, Y / W, Z / W, 1);
        }

        public static explicit operator Point(Vector4D vector)
        {
            return new Point(vector.X, vector.Y);
        }

        public static Vector4D operator *(Matrix4X4 transformation, Vector4D vector)
        {
            return vector.Transform(transformation);
        }

        public Vector4D Normalize()
        {
            var l = Length;
            return new Vector4D(X/l, Y/l, Z/l, W/l);
        }

        public double DotProduct(Vector4D v)
        {
            return X*v.X + Y*v.Y + Z*v.Z + W*v.Z;
        }
    }
}