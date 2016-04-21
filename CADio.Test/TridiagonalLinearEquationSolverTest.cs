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
            var output = LinearEquationSolver.SolveTDMA(mat, v);
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
            var output = LinearEquationSolver.SolveTDMA(mat, v);
        }

        [TestMethod]
        public void Testbandec()
        {
            const double unu = 0.0;
            var mat = new[,]
            {
                {  unu, 3.0, 1.0 },
                {  2.0, 3.0, 0.0 },
                { -1.0, 3.0, 1.0 },
                {  0.5, 3.0, unu },
            };

            var v = new[] { 14.0, 21.0, 27.0, 36.5 };

            double[,] up;
            int[] indx;
            double d;

            LinearEquationSolver.bandec(mat, 4, 1, 1, out up, out indx, out d);
            LinearEquationSolver.banbks(mat, 4, 1, 1, up, indx, v);
        }

    }
}
