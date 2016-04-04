using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CADio.Mathematics
{
    public static class Transformations3D
    {
        public static Matrix4X4 Translation(Vector3D v)
        {
            return new Matrix4X4
            {
                Cells = new [,]
                {
                    {    1,    0,    0,  v.X },
                    {    0,    1,    0,  v.Y },
                    {    0,    0,    1,  v.Z },
                    {    0,    0,    0,    1 },
                }
            };
        }

        public static Matrix4X4 Scaling(double scale)
        {
            return Scaling(new Vector3D(scale, scale, scale));
        }

        public static Matrix4X4 Scaling(Vector3D scale)
        {
            return new Matrix4X4
            {
                Cells = new [,]
                {
                    { scale.X,       0,       0,       0 },
                    {       0, scale.Y,       0,       0 },
                    {       0,       0, scale.Z,       0 },
                    {       0,       0,       0,       1 },
                }
            };
        }

        public static Matrix4X4 RotationX(double angle)
        {
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);

            return new Matrix4X4
            {
                Cells = new [,]
                {
                    {    1,    0,    0,    0 },
                    {    0,  cos, -sin,    0 },
                    {    0,  sin,  cos,    0 },
                    {    0,    0,    0,    1 },
                }
            };
        }

        public static Matrix4X4 RotationY(double angle)
        {
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);

            return new Matrix4X4
            {
                Cells = new [,]
                {
                    {  cos,    0,  sin,    0 },
                    {    0,    1,    0,    0 },
                    { -sin,    0,  cos,    0 },
                    {    0,    0,    0,    1 },
                }
            };
        }

        public static Matrix4X4 RotationZ(double angle)
        {
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);

            return new Matrix4X4
            {
                Cells = new[,]
                {
                    {  cos, -sin,    0,    0 },
                    {  sin,  cos,    0,    0 },
                    {    0,    0,    1,    0 },
                    {    0,    0,    0,    1 },
                }
            };
        }

        public static Matrix4X4 SimplePerspective(double r, double aspect)
        {
            var a = 1.0 / aspect;
            var q = 1/r;

            return new Matrix4X4
            {
                Cells = new [,]
                {
                    {a, 0, 0, 0},
                    {0, 1, 0, 0},
                    {0, 0, 0, 0},
                    {0, 0, q, 1},
                }
            };
        }

        public static Matrix4X4 SimplePerspectiveWithEyeShift(double r, double aspect, double eyeShift)
        {
            var a = 1.0/aspect;
            var p = eyeShift/(2*r);
            var q = 1 / r;

            return new Matrix4X4
            {
                Cells = new[,]
                {
                    {a, 0, p, 0},
                    {0, 1, 0, 0},
                    {0, 0, 0, 0},
                    {0, 0, q, 1},
                }
            };
        }
    }
}