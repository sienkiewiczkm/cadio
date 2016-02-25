using System.Windows.Media.Media3D;

namespace CADio.Mathematics
{
    public static class Vector3DExtensions
    {
        public static Vector4D ExtendAffine(this Vector3D vector)
        {
            return new Vector4D
            {
                X = vector.X,
                Y = vector.Y,
                Z = vector.Z,
                W = 1,
            };
        }
    }
}