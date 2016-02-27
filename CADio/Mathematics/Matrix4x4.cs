using System;

namespace CADio.Mathematics
{
    public class Matrix4x4
    {
        private double[,] _cells = new double[4, 4];

        public double[,] Cells
        {
            get { return _cells; }
            set
            {
                if (value.GetLength(0) != 4 || value.GetLength(1) != 4)
                    throw new ArgumentException("Only 4x4 arrays are assignable to " + nameof(Matrix4x4));
                _cells = value;
            }
        }

        public static Matrix4x4 Identity
        {
            get
            {
                var matrix = new Matrix4x4();
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

        public Matrix4x4 Multiply(Matrix4x4 matrix)
        {
            var newMatrix = new Matrix4x4();
            for (int row = 0; row < 4; ++row)
                for (int column = 0; column < 4; ++column)
                    for (int i = 0; i < 4; ++i)
                        newMatrix[row, column] += this[row, i] * matrix[i, column];
            return newMatrix;
        }

        public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
        {
            return a.Multiply(b);
        }
    }
}