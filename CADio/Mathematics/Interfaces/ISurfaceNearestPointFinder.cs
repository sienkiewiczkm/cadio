using System.Windows.Media.Media3D;

namespace CADio.Mathematics.Interfaces
{
    public interface ISurfaceNearestPointFinder
    {
        Parametrisation FindNearest(
            IParametricSurface surface,
            Point3D referencePoint
        );
    }
}