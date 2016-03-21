using System;
using System.Windows.Media.Media3D;
using CADio.Mathematics;

namespace CADio.Rendering
{
    public class Camera
    {
        protected static double MinZoom = 0.1;

        private double _xRotation;
        private double _yRotation;
        private double _zoom = 1.0;

        public Point3D Position { get; set; }
        public double ObserverOffset { get; set; }

        public double Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = value;
                if (_zoom < MinZoom)
                    _zoom = MinZoom;
            }
        }

        public double XRotation
        {
            get { return _xRotation; }
            set
            {
                var pi2 = Math.PI*0.5;
                _xRotation = MathHelpers.Clamp(value, -pi2, pi2);
            }
        }

        public double YRotation
        {
            get { return _yRotation; }
            set
            {
                _yRotation = value;
                if (_yRotation > 2*Math.PI)
                    _yRotation -= 2*Math.PI;
                if (_yRotation < 0)
                    _yRotation += 2*Math.PI;
            }
        }

        public void Move(Vector3D relativeMove)
        {
            var view = GetViewMatrix();
            var right = new Vector3D(view[0, 0], view[0, 1], view[0, 2]);
            var forward = new Vector3D(view[2, 0], view[2, 1], view[2, 2]);
            Position += right*relativeMove.X + forward*relativeMove.Z;
        }

        public Matrix4X4 GetViewMatrix()
        {
            var scale = Transformations3D.Scaling(Zoom);
            var translation = Transformations3D.Translation(-(Vector3D)Position);
            var rotation = Transformations3D.RotationX(XRotation)*
                           Transformations3D.RotationY(YRotation);
            var d = Transformations3D.Translation(new Vector3D(0, 0, ObserverOffset));
            var invd = Transformations3D.Translation(new Vector3D(0, 0, -ObserverOffset));
            return scale*invd*rotation*d*translation;
        }
    }
}