namespace CADio.Helpers
{
    public static class ArrayHelpers
    {
        public static T[] GetRow<T>(T[,] array, int row)
        {
            var elements = array.GetLength(1);
            var output = new T[elements];
            for (var i = 0; i < elements; ++i)
                output[i] = array[row, i];
            return output;
        }
        
        public static T[] GetColumn<T>(T[,] array, int column)
        {
            var elements = array.GetLength(0);
            var output = new T[elements];
            for (var i = 0; i < elements; ++i)
                output[i] = array[i, column];
            return output;
        }
    }
}