using System.Windows.Media.Media3D;
using CADio.Mathematics;

namespace CADio.Rendering
{
    public class ArcBallCamera : ICamera
    {
        private Vector3D _rotation;

        public double Zoom { get; set; }

        public Matrix4X4 GetViewMatrix()
        {
            throw new System.NotImplementedException();
        }

        public Matrix4X4 GetPerspectiveMatrix(double xEyeShift = 0)
        {
            return Transformations3D.SimplePerspectiveWithEyeShift(2.0, xEyeShift);
        }

        public void Move(Vector3D relativeMove)
        {
            throw new System.NotImplementedException();
        }

        public void Rotate(Vector3D relativeRotation)
        {
            throw new System.NotImplementedException();
        }
    }
}