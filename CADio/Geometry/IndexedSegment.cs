namespace CADio.Geometry
{
    public struct IndexedSegment
    {
        public int First;
        public int Second;

        public IndexedSegment(int first, int second)
        {
            First = first;
            Second = second;
        }
    }
}