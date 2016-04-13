using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes;
using CADio.Mathematics;

namespace CADio.SceneManagement
{
    public interface IWorldObject
    {
        bool IsSelected { get; }
        string Name { get; set; }
        Vector3D Orientation { get; set; }
        Point3D Position { get; set; }
        Vector3D Scale { get; }
        IShape Shape { get; }
        bool IsGrabbed { get; }
        Scene SceneManager { get; set; }

        ICollection<ISceneSelectable> GetSelectableChildren();
        Matrix4X4 GetWorldMatrix();
        void PrerenderUpdate();
        void Translate(Vector3D translation);
        void DetachFromCompositors();
        event PropertyChangedEventHandler PropertyChanged;
    }
}