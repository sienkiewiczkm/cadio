using System.Windows.Media.Media3D;

namespace CADio.Mathematics.Interfaces
{
    public enum DerivativeParameter
    {
        U,
        V,
    }

    public interface IParametricSurface
    {
        Point3D Evaluate(double u, double v);
        Vector3D Derivative(double u, double v, DerivativeParameter parameter);
    }
}