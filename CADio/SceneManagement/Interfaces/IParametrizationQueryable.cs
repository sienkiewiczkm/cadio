using CADio.Mathematics.Interfaces;

namespace CADio.SceneManagement.Interfaces
{
    public interface IParametrizationQueryable
    {
        string Name { get; }
        IParametricSurface GetParametricSurface();
    }
}