using System;

namespace CADio.Mathematics.Trimming
{
    public class SurfaceTimmerLookup:
        ISurfaceTrimmer
    {
        private readonly bool[,] _lookupTable;

        public SurfaceTimmerLookup(bool[,] lookupTable)
        {
            _lookupTable = lookupTable;
        }

        public TrimMode TrimMode { get; set; }

        public bool VerifyParametrisation(double u, double v)
        {
            if (TrimMode == TrimMode.Disabled)
                return true;

            var vLookups = _lookupTable.GetLength(0);
            var uLookups = _lookupTable.GetLength(1);

            var uCell = Math.Min(uLookups - 1, (int) (u*uLookups));
            var vCell = Math.Min(vLookups - 1, (int) (v*vLookups));

            var value = _lookupTable[vCell, uCell];

            if (TrimMode == TrimMode.Outside)
                return !value;
            return value;
        }

        public static SurfaceTimmerLookup CreateBasedOn(
            SurfaceTrimmer trimmer,
            int uLookups,
            int vLookups
            )
        {
            var oldTrimMode = trimmer.TrimMode;
            trimmer.TrimMode = TrimMode.Inside;
            
            var lookupTable = new bool[vLookups, uLookups];
            for (var v = 0; v < vLookups; ++v)
            {
                var fillFalse = false;
                for (var u = 0; u < uLookups; ++u)
                {
                    var uParam = ((double) u)/(uLookups - 1);
                    var vParam = ((double) v)/(vLookups - 1);

                    if (fillFalse)
                    {
                        lookupTable[v, u] = false;
                    }
                    else
                    {
                        lookupTable[v, u] = trimmer.VerifyParametrisation(
                            uParam,
                            vParam
                            );
                    }

                    if (trimmer.WasZeroIntersections)
                        fillFalse = true;
                }
            }

            trimmer.TrimMode = oldTrimMode;

            return new SurfaceTimmerLookup(lookupTable);
        }
    }
}