using System.Windows.Media.Media3D;

namespace CADio.SceneManagement
{
    public class VirtualPoint : ISceneSelectable
    {
        public Point3D Position { get; set; }
        public bool IsGrabbed { get; set; }

        public void Translate(Vector3D translation)
        {
            Position += translation;
        }
    }
}