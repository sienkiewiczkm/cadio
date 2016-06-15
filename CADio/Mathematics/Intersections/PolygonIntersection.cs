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

        public bool IsParameterAlreadyFound(
            IntersectionParametrisation parametrisation
            )
        {
            foreach (var param in Polygon)
            {
                var norm = IntersectionParametrisation.DistanceNormMax(
                    param,
                    parametrisation
                );

                if (norm < EqualityEpsilon)
                    return true;
            }

            return false;
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
    }
}
