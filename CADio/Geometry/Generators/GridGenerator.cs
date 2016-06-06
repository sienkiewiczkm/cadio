using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace CADio.Geometry.Generators
{
    public static class GridGenerator
    {
        public static List<Point3D> Generate(
            int segmentsU,
            int segmentsV,
            double width = 1,
            double height = 1
            )
        {
            var spacingX = 1.0 / (segmentsU - 1);
            var spacingY = 1.0 / (segmentsV - 1);
            var totalX = spacingX * (segmentsU - 1);
            var totalY = spacingY * (segmentsV - 1);

            var points = new List<Point3D>();

            for (var y = 0; y < segmentsV; y++)
            {
                for (var x = 0; x < segmentsU; x++)
                {
                    points.Add(
                         new Point3D(
                            width * (x * spacingX - totalX * 0.5),
                            0,
                            height * (y * spacingY - totalY * 0.5)
                        )
                    );
                }
            }

            return points;
        }
    }
}