using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes;
using CADio.Geometry.Shapes.Static;
using CADio.Mathematics;
using CADio.SceneManagement.Interfaces;

namespace CADio.SceneManagement
{
    public class MarkerPointObject : IWorldObject, ISceneSelectable
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }

        public IShape Shape => new MarkerPoint();

        public Point3D Position { get; set; }
        public Point3D WorldPosition => Position;
        public Vector3D Scale { get; set; }
        public bool IsGrabbed { get; set; }
        public Scene SceneManager { get; set; }
        public Vector3D Orientation { get; set; }

        public ICollection<ISceneSelectable> GetSelectableChildren() => new List<ISceneSelectable>(); 

        public Matrix4X4 GetWorldMatrix()
        {
            return Matrix4X4.Identity;
        }

        public void PrerenderUpdate()
        {
            throw new NotImplementedException();
        }

        public void Translate(Vector3D translation)
        {
            throw new NotImplementedException();
        }

        public void DetachFromCompositors()
        {
            throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
