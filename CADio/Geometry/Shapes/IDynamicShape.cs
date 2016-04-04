using System;
using System.Windows;
using System.Windows.Media.Media3D;

namespace CADio.Geometry.Shapes
{
    public interface IDynamicShape : IShape
    {
        void UpdateGeometry(Func<Point3D, Point3D, double> estimateScreenSpaceDistance);
    }
}