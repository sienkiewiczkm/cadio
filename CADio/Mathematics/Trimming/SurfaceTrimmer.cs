using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Media3D;
using CADio.Mathematics.Interfaces;
using CADio.Mathematics.Intersections;

namespace CADio.Mathematics.Trimming
{
    public class SurfaceTrimmer
    {
        private List<TrimmingArea> _trimmingAreas = new List<TrimmingArea>();

        public void BuildTrimmingAreas(
            PolygonIntersection intersection,
            Func<IntersectionParametrisation, Parametrisation> paramSelector
            )
        {
            if (!intersection.IsLooped)
                return;

            var area = new TrimmingArea();
            for (var i = 0; i < intersection.Polygon.Count; ++i)
            {
                var parametrisation = paramSelector(intersection.Polygon[i]);
                area.Polygon.Add(
                    new Point(parametrisation.U, parametrisation.V)
                );
            }

            _trimmingAreas.Add(area);
        }

        public bool VerifyParametrisation(double u, double v)
        {
            var rayOrigin = new Point(u, v);
            var rayTarget = new Point(u+2, v);

            foreach (var trimmingArea in _trimmingAreas)
            {
                var trimmingPointsNum = trimmingArea.Polygon.Count;
                var intersections = 0;

                var lastIntersection = SegmentIntersectionKind.None;
                for (var i = 0; i < trimmingPointsNum - 1; ++i)
                {
                    var pt1 = trimmingArea.Polygon[i];
                    var pt2 = trimmingArea.Polygon[i + 1];

                    var intersectionKind =
                        GeometricIntersections.CheckRayLineSegmentCollision(
                            rayOrigin,
                            rayTarget,
                            pt1,
                            pt2
                        );

                    if (intersectionKind != SegmentIntersectionKind.None &&
                        !(lastIntersection == SegmentIntersectionKind.Ending
                        && intersectionKind == SegmentIntersectionKind.Beginning
                        ))
                    {
                        ++intersections;
                    }

                    lastIntersection = intersectionKind;
                }

                if (intersections%2 == 1)
                    return false;
            }

            return true;
        }

        private class TrimmingArea
        {
            public List<Point> Polygon { get; set; } = new List<Point>();
        }
    }
}