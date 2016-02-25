using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CADio.Mathematics
{
    public static class Transformations3D
    {
        public static Matrix4x4 Translation(Vector3D v)
        {
            return new Matrix4x4
            {
                Cells = new double[4, 4]
                {
                    {    1,    0,    0,  v.X },
                    {    0,    1,    0,  v.Y },
                    {    0,    0,    1,  v.Z },
                    {    0,    0,    0,    1 },
                }
            };
        }

        public static Matrix4x4 RotationX(double angle)
        {
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);

            return new Matrix4x4
            {
                Cells = new double[4, 4]
                {
                    {    1,    0,    0,    0 },
                    {    0,  cos, -sin,    0 },
                    {    0,  sin,  cos,    0 },
                    {    0,    0,    0,    1 },
                }
            };
        }

        public static Matrix4x4 RotationY(double angle)
        {
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);

            return new Matrix4x4
            {
                Cells = new double[4, 4]
                {
                    {  cos,    0,  sin,    0 },
                    {    0,    1,    0,    0 },
                    { -sin,    0,  cos,    0 },
                    {    0,    0,    0,    1 },
                }
            };
        }

        public static Matrix4x4 SimplePerspective(double r)
        {
            var q = 1/r;

            return new Matrix4x4
            {
                Cells = new double[4, 4]
                {
                    {1, 0, 0, 0},
                    {0, 1, 0, 0},
                    {0, 0, 0, 0},
                    {0, 0, q, 1},
                }
            };
        }
    }
}