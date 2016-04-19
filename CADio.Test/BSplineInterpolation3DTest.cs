using System.Collections.Generic;
using System.Windows.Media.Media3D;
using CADio.Mathematics.Numerical;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CADio.Test
{
    [TestClass]
    public class BSplineInterpolation3DTest
    {
        [TestMethod]
        public void MultiplyMatrixAndIdentity_ReturnsUnchanged()
        {
            var cp = new List<Point3D>()
            {
                new Point3D(-2, 0, 0),
                new Point3D(-1, 2, 0),
                new Point3D(0, 1, 0),
                new Point3D(+1, 3, 0),
                new Point3D(+2, 6, 0),
                new Point3D(+3, 7, 0),
            };

            BSplineInterpolation3D.Interpolate(cp);
        }
    }
}