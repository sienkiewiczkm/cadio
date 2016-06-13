using System.Windows.Media.Media3D;
using MathNet.Numerics.LinearAlgebra.Double;

namespace CADio.Mathematics
{
    public static class Vector3DExtensions
    {
        public static Vector4D ExtendTo4D(this Vector3D vector, double extension = 1)
        {
            return new Vector4D
            {
                X = vector.X,
                Y = vector.Y,
                Z = vector.Z,
                W = extension,
            };
        }

        public static DenseVector ToMathVector(this Vector3D vector)
        {
            return DenseVector.OfArray(
                new []
                {
                    vector.X,
                    vector.Y,
                    vector.Z
                }
            );
        } 
    }
}