using System;
using CADio.Mathematics;
using CADio.RayCaster.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CADio.RayCaster.Test
{
    [TestClass]
    public class ImplicitEllipsoidEquationSolverTest
    {
        [TestMethod]
        public void TrySolveSimpleQuadraticEquation()
        {
            var mat = new Matrix4x4()
            {
                Cells = new double[4, 4]
                {
                    {-1,  0, 0,  0},
                    { 0, -1, 0,  0},
                    { 0,  0, 1,  0},
                    { 0,  0, 0, -2}
                }
            };

            var v = new Vector4D(1, 1, 0, 1);

            var z = ImplicitEllipsoidEquationSolver.SolveZ(v, mat);

            Assert.IsTrue(z.HasValue);
            Assert.AreEqual(-2.0, z.Value, 0.05);
        }
    }
}
