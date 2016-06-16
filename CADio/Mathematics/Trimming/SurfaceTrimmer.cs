using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using CADio.Mathematics.Interfaces;
using CADio.Mathematics.Intersections;

namespace CADio.Mathematics.Trimming
{
    public class SurfaceTrimmer : ISurfaceTrimmer
    {
        public const double SnapThreshold = 0.05;

        private List<TrimmingArea> _trimmingAreas = new List<TrimmingArea>();

        public void BuildTrimmingAreas(
            PolygonIntersection intersection,
            Func<IntersectionParametrisation, Parametrisation> paramSelector
            )
        {
            if (intersection.Polygon.Count == 0)
                return;

            var area = new TrimmingArea();

            Point? leftSnap = null;

            var firstParam = paramSelector(intersection.Polygon[0]);
            if (!intersection.IsLooped && 
                (IsSnappable(firstParam.U) || IsSnappable(firstParam.V))
                )
            {
                leftSnap = new Point(Snap(firstParam.U), Snap(firstParam.V));
                area.Polygon.Add(leftSnap.Value);
            }

            Parametrisation? previous = null;
            for (var i = 0; i < intersection.Polygon.Count; ++i)
            {
                var parametrisation = paramSelector(intersection.Polygon[i]);

                // skip v traversal
                if (parametrisation.Boundaries.AllowTraversalV &&
                    previous.HasValue &&
                    previous.Value.VBoxId != parametrisation.VBoxId)
                {
                   // subdivide this part 
                    Func<double, double, double, double> tFunc =
                        (side, x0, dx) => (side - x0)/dx;

                    var prevUnbU = previous.Value.UnboundedU;
                    var prevUnbV = previous.Value.UnboundedV;

                    var dU = parametrisation.UnboundedU - prevUnbU;
                    var dV = parametrisation.UnboundedV - prevUnbV;

                    var t1 = tFunc(0, prevUnbV, dV);
                    var t2 = tFunc(1, prevUnbV, dV);
                    var t = Math.Max(t1, t2);

                    var subdivideIncreaseU = dU*t;
                    var subdivideIncreaseV = dV*t;

                    var firstSubdividePoint = new Point(
                        previous.Value.U + subdivideIncreaseU,
                        previous.Value.V + subdivideIncreaseV
                    );

                    var secondSubdividePoint = new Point(
                        firstSubdividePoint.X, 
                        1 - firstSubdividePoint.Y
                    );

                    area.Polygon.Add(firstSubdividePoint);
                    area.Polygon.Add(secondSubdividePoint);
                }

                area.Polygon.Add(
                    new Point(parametrisation.U, parametrisation.V)
                );
                previous = parametrisation;
            }

            var lastParam = paramSelector(intersection.Polygon.Last());
            if (!intersection.IsLooped && 
                (IsSnappable(lastParam.U) || IsSnappable(lastParam.V))
                )
            {
                Point rightSnap = new Point(
                    Snap(lastParam.U), 
                    Snap(lastParam.V)
                );
                area.Polygon.Add(rightSnap);

                if (leftSnap.HasValue)
                {
                    bool top, bottom, right, left;
                    top = bottom = right = left = false;

                    top |= AlmostEqual(leftSnap.Value.Y, 0.0);
                    top |= AlmostEqual(rightSnap.Y, 0.0);
                    bottom |= AlmostEqual(leftSnap.Value.Y, 1.0);
                    bottom |= AlmostEqual(rightSnap.Y, 1.0);

                    left |= AlmostEqual(leftSnap.Value.X, 0.0);
                    left |= AlmostEqual(rightSnap.X, 0.0);
                    right |= AlmostEqual(leftSnap.Value.X, 1.0);
                    right |= AlmostEqual(rightSnap.X, 1.0);

                    Point? additional = null;
                    if (top && bottom || left && right)
                    {
                        
                    }
                    else
                    {
                        var x = left ? 0.0 : 1.0;
                        var y = top ? 0.0 : 1.0;
                        additional = new Point(x, y);
                    }

                    if (additional.HasValue)
                    {
                        area.Polygon.Add(additional.Value);
                        area.Polygon.Add(leftSnap.Value);
                    }
                }
            }

            _trimmingAreas.Add(area);
        }

        private double Snap(double d)
        {
            if (Math.Abs(d) < SnapThreshold)
                return 0.0;
            if (Math.Abs(1.0 - d) < SnapThreshold)
                return 1.0;
            return d;
        }

        private bool IsSnappable(double intersectionParametrisation)
        {
            if (Math.Abs(intersectionParametrisation) < SnapThreshold)
                return true;
            if (Math.Abs(1.0 - intersectionParametrisation) < SnapThreshold)
                return true;
            return false;
        }

        private bool AlmostEqual(double a, double b)
        {
            return Math.Abs(a - b) < SnapThreshold;
        }

        public TrimMode TrimMode { get; set; }

        public bool WasZeroIntersections = false;
        public bool VerifyParametrisation(double u, double v)
        {
            WasZeroIntersections = false;
            if (TrimMode == TrimMode.Disabled)
                return true;
            var valueToReturnIfInside = TrimMode == TrimMode.Outside;

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

                    // skip traversals
                    if (Math.Abs(pt1.X - pt2.X) < 0.000001 && 
                        Math.Abs(pt1.Y + pt2.Y - 1) < 0.0001)
                        continue;

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
                    return valueToReturnIfInside;

                if (intersections == 0)
                    WasZeroIntersections = true;
            }

            return !valueToReturnIfInside;
        }

        private class TrimmingArea
        {
            public List<Point> Polygon { get; set; } = new List<Point>();
        }
    }
}