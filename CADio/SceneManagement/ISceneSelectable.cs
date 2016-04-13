using System.Windows.Media.Media3D;

namespace CADio.SceneManagement
{
    public interface ISceneSelectable
    {
        Point3D Position { get; }
        bool IsGrabbed { get; set; }
        void Translate(Vector3D translation);
    }
}