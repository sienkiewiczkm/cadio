using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace CADio.Mathematics.Numerical
{
    public class BSplineToBernsteinConverter
    {
        public static IList<Point3D> ConvertAssumingEquidistantKnots(IList<Point3D> deBoorPoints)
        {
            var refinedList = new List<Point3D>();
            for (var i = 0; i < deBoorPoints.Count - 1; ++i)
            {
                var pt1 = deBoorPoints[i];
                var pt2 = deBoorPoints[i + 1];

                var innerBernstein1 = MathHelpers.Lerp(pt1, pt2, 1.0/3);
                var innerBernstein2 = MathHelpers.Lerp(pt1, pt2, 2.0/3);

                refinedList.Add(innerBernstein1);
                refinedList.Add(innerBernstein2);
            }

            var bernsteinList = new List<Point3D>();
            for (var i = 2; i < refinedList.Count - 1; ++i)
            {
                var curr = refinedList[i];

                if (i%2 == 0)
                {
                    var prev = refinedList[i - 1];
                    var additional = MathHelpers.Lerp(prev, curr, 0.5);
                    bernsteinList.Add(additional);
                }

                // Exclude last two points
                if (i < refinedList.Count - 2)
                    bernsteinList.Add(curr);
            }

            return bernsteinList;
        }
    }
}
