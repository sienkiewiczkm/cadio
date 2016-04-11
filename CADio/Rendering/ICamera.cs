using System.Windows.Media.Media3D;
using CADio.Mathematics;

namespace CADio.Rendering
{
    public interface ICamera
    {
        double Zoom { get; set; }

        Matrix4X4 GetViewMatrix();
        Matrix4X4 GetPerspectiveMatrix(double xEyeShift = 0.0);

        void Move(Vector3D relativeMove);
        void Rotate(Vector3D relativeRotation);
    }
}