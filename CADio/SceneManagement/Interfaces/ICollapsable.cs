using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace CADio.SceneManagement.Interfaces
{
    public interface ICollapsable
    {
        Point3D Position { get; set; }
        ICollapsable Tracked { get; set; }
        List<ICollapsable> Trackers { get; }
    }
}