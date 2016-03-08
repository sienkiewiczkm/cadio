using System;

namespace CADio.Mathematics
{
    public class Matrix4X4
    {
        private double[,] _cells = new double[4, 4];

        public double[,] Cells
        {
            get { return _cells; }
            set
            {
                if (value.GetLength(0) != 4 || value.GetLength(1) != 4)
                    throw new ArgumentException("Only 4x4 arrays are assignable to " + nameof(Matrix4X4));
                _cells = value;
            }
        }

        public static Matrix4X4 Identity
        {
            get
            {
                var matrix = new Matrix4X4();
                for (int i = 0; i < 4; ++i)
                    matrix[i, i] = 1;
                return matrix;
            }
        }

        public double this[int row, int column]
        {
            get { return Cells[row, column]; }
            set { Cells[row, column] = value; }
        }

        public Matrix4X4 Multiply(Matrix4X4 matrix)
        {
            var newMatrix = new Matrix4X4();
            for (var row = 0; row < 4; ++row)
                for (var column = 0; column < 4; ++column)
                    for (var i = 0; i < 4; ++i)
                        newMatrix[row, column] += this[row, i] * matrix[i, column];
            return newMatrix;
        }

        public Matrix4X4 Transpose()
        {
            var ret = new Matrix4X4();
            for (var i = 0; i < 4; ++i)
                for (var j = 0; j < 4; ++j)
                    ret[i, j] = Cells[j, i];
            return ret;
        }

        public Matrix4X4 GaussianInverse()
        {
            var augumentedMatrix = BuildIdentityAugumentedMatrix();
            ExecuteGaussianElimination(augumentedMatrix);
            ReduceAugumentedUpperMatrixToDiagonal(augumentedMatrix);
            DivideAugumentedMatrixByDiagonalValues(augumentedMatrix);
            return GetOutputMatrix(augumentedMatrix);
        }

        private static void ExecuteGaussianElimination(double[,] augumentedMatrix)
        {
            for (var k = 0; k < 4; ++k)
            {
                SwapRowsIfColumnMaximumIsLowerThanCurrent(augumentedMatrix, k);

                for (var i = k + 1; i < 4; ++i)
                {
                    var m = augumentedMatrix[i, k]/augumentedMatrix[k, k];
                    for (var j = k + 1; j < 8; ++j)
                    {
                        augumentedMatrix[i, j] = augumentedMatrix[i, j] - augumentedMatrix[k, j]*m;
                    }
                    augumentedMatrix[i, k] = 0;
                }
            }
        }

        private static void SwapRowsIfColumnMaximumIsLowerThanCurrent(double[,] augumentedMatrix, int currentRow)
        {
            var maxi = currentRow;
            var maxAbsVal = Math.Abs(augumentedMatrix[maxi, currentRow]);

            for (var i = maxi + 1; i < 4; ++i)
            {
                var candidate = Math.Abs(augumentedMatrix[i, currentRow]);
                if (candidate <= maxAbsVal) continue;

                maxi = i;
                maxAbsVal = candidate;
            }

            if (Math.Abs(augumentedMatrix[maxi, currentRow]) < 1E-15)
                throw new ArgumentException("Matrix is singular.");

            if (maxi != currentRow)
            {
                for (var i = 0; i < 8; ++i)
                {
                    var temp = augumentedMatrix[maxi, i];
                    augumentedMatrix[maxi, i] = augumentedMatrix[currentRow, i];
                    augumentedMatrix[currentRow, i] = temp;
                }
            }
        }

        private double[,] BuildIdentityAugumentedMatrix()
        {
            var augumentedMatrix = new double[4, 8];
            var identity = Matrix4X4.Identity;

            for (var i = 0; i < 4; ++i)
            {
                for (var j = 0; j < 4; ++j)
                {
                    augumentedMatrix[i, j] = Cells[i, j];
                    augumentedMatrix[i, 4 + j] = identity[i, j];
                }
            }
            return augumentedMatrix;
        }

        private static void ReduceAugumentedUpperMatrixToDiagonal(double[,] augumentedMatrix)
        {
            for (var k = 3; k >= 0; --k)
            {
                for (var i = 0; i < k; ++i)
                {
                    var m = augumentedMatrix[i, k]/augumentedMatrix[k, k];
                    for (var j = k + 1; j < 8; ++j)
                        augumentedMatrix[i, j] = augumentedMatrix[i, j] - augumentedMatrix[k, j]*m;
                    augumentedMatrix[i, k] = 0;
                }
            }
        }

        private static void DivideAugumentedMatrixByDiagonalValues(double[,] augumentedMatrix)
        {
            for (var k = 0; k < 4; ++k)
            {
                var divider = augumentedMatrix[k, k];
                for (var i = k + 1; i < 8; ++i)
                    augumentedMatrix[k, i] /= divider;
                augumentedMatrix[k, k] = 1;
            }
        }

        private static Matrix4X4 GetOutputMatrix(double[,] augumentedMatrix)
        {
            var retMatrix = new Matrix4X4();
            for (var i = 0; i < 4; ++i)
                for (var j = 0; j < 4; ++j)
                    retMatrix[i, j] = augumentedMatrix[i, j + 4];
            return retMatrix;
        }

        public static Matrix4X4 operator *(Matrix4X4 a, Matrix4X4 b)
        {
            return a.Multiply(b);
        }
    }
}