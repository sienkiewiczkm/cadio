namespace CADio.Geometry
{
    public struct IndexedLine
    {
        public int First;
        public int Second;

        public IndexedLine(int first, int second)
        {
            First = first;
            Second = second;
        }
    }
}