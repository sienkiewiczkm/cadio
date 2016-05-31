using System.Windows.Media.Media3D;

namespace CADio.Mathematics.Interfaces
{
    public interface IParametricSurface
    {
        Point3D Evaluate(double u, double v);
    }
}