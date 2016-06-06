using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace CADio.Geometry.Generators
{
    public static class CylinderGenerator
    {
        public static List<Point3D> Generate(
            int lengthPoints,
            int circlePoints,
            double radius,
            double height
            )
        {
            var points = new List<Point3D>();

            for (var j = 0; j < circlePoints; ++j)
            {
                var angle = 2 * Math.PI * j / circlePoints;
                var sina = Math.Sin(angle);
                var cosa = Math.Cos(angle);
                for (var i = 0; i < lengthPoints; ++i)
                {
                    var h = height * i / lengthPoints;
                    points.Add(new Point3D(radius * sina, h, radius * cosa));
                }
            }

            return points;
        }
    }
}