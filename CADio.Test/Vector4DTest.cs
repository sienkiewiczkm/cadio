using System;
using CADio.Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CADio.Test
{
    [TestClass]
    public class Vector4DTest
    {
        [TestMethod]
        public void Transform_IdentityShouldNotChangeVector()
        {
            var vector = new Vector4D(1, 2, 3, 4);
            var transformation = Matrix4X4.Identity;
            var outputVector = transformation*vector;

            Assert.AreEqual(vector.X, outputVector.X);
            Assert.AreEqual(vector.Y, outputVector.Y);
            Assert.AreEqual(vector.Z, outputVector.Z);
            Assert.AreEqual(vector.W, outputVector.W);
        }

        [TestMethod]
        public void Transform_SwapCoordinates()
        {
            var transform = new Matrix4X4()
            {
                Cells = new double[4, 4]
                {
                    { 0, 1, 0, 0 },
                    { 0, 0, 1, 0 },
                    { 0, 0, 0, 1 },
                    { 1, 0, 0, 0 }
                }
            };

            var vector = new Vector4D(1, 2, 3, 4);
            var result = transform*vector;

            Assert.AreEqual(result.X, vector.Y);
            Assert.AreEqual(result.Y, vector.Z);
            Assert.AreEqual(result.Z, vector.W);
            Assert.AreEqual(result.W, vector.X);
        }
    }
}