using System;
using CADio.Mathematics.Numerical;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CADio.Test
{
    [TestClass]
    public class TridiagonalLinearEquationSolverTest
    {
        [TestMethod]
        public void SolveSimpleEquation()
        {
            var mat = new [,]
            {
                { 1.0, 0.0, 0.0, 0.0 },
                { 0.0, 1.0, 0.0, 0.0 },
                { 0.0, 0.0, 1.0, 0.0 },
                { 0.0, 0.0, 0.0, 1.0 },
            };

            var v = new[] {3.0, 5.0, 7.0, 8.0};
            var output = TridiagonalLinearEquationSolver.SolveTDMA(mat, v);
        }

        [TestMethod]
        public void SolveSimpleEquation2()
        {
            var mat = new[,]
            {
                { 3.0,  1.0, 0.0, 0.0 },
                { 2.0,  3.0, 0.0, 0.0 },
                { 0.0, -1.0, 3.0, 1.0 },
                { 0.0,  0.0, 0.5, 3.0 },
            };

            var v = new[] { 14.0, 21.0, 27.0, 36.5 };
            var output = TridiagonalLinearEquationSolver.SolveTDMA(mat, v);
        }
    }
}
