using System;
using System.Windows.Media.Media3D;
using CADio.Mathematics;

namespace CADio.Rendering
{
    public class ArcBallCamera : ICamera
    {
        private double _xRotation;
        private double _yRotation;
        private double _zoom = 1.0;

        public double Zoom
        {
            get { return _zoom; }
            set { _zoom = Math.Max(0.1, value); }
        }

        public double XRotation
        {
            get { return _xRotation; }
            set
            {
                const double pi2 = Math.PI * 0.5;
                _xRotation = MathHelpers.Clamp(value, -pi2, pi2);
            }
        }

        public double YRotation
        {
            get { return _yRotation; }
            set
            {
                _yRotation = value;
                if (_yRotation > 2 * Math.PI)
                    _yRotation -= 2 * Math.PI;
                if (_yRotation < 0)
                    _yRotation += 2 * Math.PI;
            }
        }

        public double Radius { get; set; } = 2.0;

        public Matrix4X4 GetViewMatrix()
        {
            var vec = new Vector3D(0, 0, -1);
            var transform = Transformations3D.Scaling(Zoom)
                            *Transformations3D.RotationY(YRotation)
                            *Transformations3D.RotationX(XRotation);
            var eye = (Point3D) (vec.ExtendTo4D().Transform(transform));

            return Transformations3D.LookAt(
                eye, 
                new Point3D(0, 0, 0), 
                new Vector3D(0, 1, 0));
        }

        public Matrix4X4 GetPerspectiveMatrix(double xEyeShift = 0)
        {
            return Transformations3D.SimplePerspectiveWithEyeShift(2.0, xEyeShift);
        }

        public void Move(Vector3D relativeMove)
        {
        }

        public void Rotate(Vector3D relativeRotation)
        {
            XRotation += relativeRotation.X;
            YRotation += relativeRotation.Y;
        }
    }
}