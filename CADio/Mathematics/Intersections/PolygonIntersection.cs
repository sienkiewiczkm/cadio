using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CADio.Mathematics.Intersections
{
    public class PolygonIntersection
    {
        public bool IsLooped { get; set; }
        public double EqualityEpsilon { get; set; } = 0.001;
        public List<IntersectionParametrisation> Polygon { get; set; } =
            new List<IntersectionParametrisation>();

        public bool FinishIfLoopedBack(
            IntersectionParametrisation parametrisation
            )
        {
            var index = FindParametrisationIndex(parametrisation);
            if (index == -1)
                return false;

            Polygon = Polygon.Skip(index).ToList();
            Polygon.Add(parametrisation);
            Polygon.Add(Polygon.First());

            IsLooped = true;
            return true;
        }

        private int FindParametrisationIndex(
            IntersectionParametrisation parametrisation
            )
        {
            for (var i = 0; i < Polygon.Count; ++i)
            {
                var norm = IntersectionParametrisation.DistanceNormMax(
                    parametrisation,
                    Polygon[i]
                );

                if (norm < EqualityEpsilon)
                    return i;
            }

            return -1;
        }

        public void Add(IntersectionParametrisation parametrisation)
        {
            Polygon.Add(parametrisation);
        }

        public static PolygonIntersection JoinFromSamePoint(
            PolygonIntersection left,
            PolygonIntersection right
            )
        {
            var rightSegment = right.Polygon.ToList();
            rightSegment.Reverse();

            var intersection = new PolygonIntersection
            {
                Polygon = rightSegment.Concat(left.Polygon).ToList()
            };
            
            return intersection;
        }

        public void FindLoopAndIsolate(IntersectionParametrisation value)
        {
        }
    }
}
