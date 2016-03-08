using System;
using System.Linq;
using CADio.Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CADio.Test
{
    [TestClass]
    public class Matrix4x4Test
    {
        [TestMethod]
        public void MultiplyMatrixAndIdentity_ReturnsUnchanged()
        {
            var matrix = new Matrix4X4()
            {
                Cells = new double[4, 4]
                {
                    { 1, 2, 3, 4 },
                    { 5, 6, 7, 8 },
                    { 9, 1, 2, 3 },
                    { 4, 5, 6, 7 }
                }
            };

            var identity = Matrix4X4.Identity;
            var output = matrix*identity;

            var equal = AreEqual(matrix, output);

            Assert.IsTrue(equal);
        }

        [TestMethod]
        public void MultiplyMatrices_ReturnsValid()
        {
            var a = new Matrix4X4()
            {
                Cells = new double[4, 4]
                {
                    { 1, 2, 3, 4 },
                    { 5, 6, 7, 8 },
                    { 9, 1, 2, 3 },
                    { 4, 5, 6, 7 }
                }
            };

            var b = new Matrix4X4()
            {
                Cells = new double[4, 4]
                {
                    { 11, 22, 34, 11 },
                    { 75, 16, 0, 43 },
                    { 3, 1, 3, 4 },
                    { 0, 0, 0, 7 }
                }
            };

            var expected = new Matrix4X4()
            {
                Cells = new double[4, 4]
                {
                    { 170,  57,  43, 137 },
                    { 526, 213, 191, 397 },
                    { 180, 216, 312, 171 },
                    { 437, 174, 154, 332 }
                }
            };

            var result = a*b;
            Assert.IsTrue(AreEqual(result, expected));
        }

        [TestMethod]
        public void InverseIdentityMatrix_ShouldBeIdentity()
        {
            var identityInversion = Matrix4X4.Identity.GaussianInverse();
            Assert.IsTrue(AreEqual(Matrix4X4.Identity, identityInversion));
        }

        [TestMethod]
        public void InverseSampleMatrix_MultiplicationOfSampleAndInverseShouldBeIdentity()
        {
            var input = new Matrix4X4()
            {
                Cells = new double[4, 4]
                {
                    {2, 1, 3, -2},
                    {-3, 2, 7, 1},
                    {8, 4, 1, 0},
                    {8, -1, 1, 3}
                }
            };

            var inputInversion = input.GaussianInverse();
            var inputInversionMulInput = inputInversion*input;
            Assert.IsTrue(AreEqual(Matrix4X4.Identity, inputInversionMulInput));
        }

        private static bool AreEqual(Matrix4X4 matrix, Matrix4X4 output, double threshold = 0.005)
        {
            for (int i = 0; i < 4; ++i)
                for (int j = 0; j < 4; ++j)
                    if (Math.Abs(matrix[i, j] - output[i, j]) > threshold)
                        return false;
            return true;
        }
    }
}
