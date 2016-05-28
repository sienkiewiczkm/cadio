using System.Windows.Media.Media3D;

namespace CADio.SceneManagement.Interfaces
{
    public interface ISceneSelectable
    {
        Point3D Position { get; }
        Point3D WorldPosition { get; }
        bool IsGrabbed { get; set; }
        void Translate(Vector3D translation);
    }
}