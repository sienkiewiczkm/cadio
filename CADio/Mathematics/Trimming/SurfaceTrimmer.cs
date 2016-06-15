﻿using System;
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

            var firstParam = paramSelector(intersection.Polygon[0]);
            if (!intersection.IsLooped && IsSnappable(firstParam.V))
            {
                area.Polygon.Add(
                    new Point(firstParam.U, Snap(firstParam.V))
                );
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
            if (!intersection.IsLooped && IsSnappable(lastParam.V))
            {
                area.Polygon.Add(
                    new Point(firstParam.U, Snap(lastParam.V))
                );
            }

            _trimmingAreas.Add(area);
        }

        private double Snap(double d)
        {
            if (Math.Abs(d) < SnapThreshold)
                return 0.0;
            if (Math.Abs(1.0 - d) < SnapThreshold)
                return 1.0;
            throw new ArgumentException("Argument not snappable");
        }

        private bool IsSnappable(double intersectionParametrisation)
        {
            if (Math.Abs(intersectionParametrisation) < SnapThreshold)
                return true;
            if (Math.Abs(1.0 - intersectionParametrisation) < SnapThreshold)
                return true;
            return false;
        }

        public TrimMode TrimMode { get; set; }

        public bool VerifyParametrisation(double u, double v)
        {
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
            }

            return !valueToReturnIfInside;
        }

        private class TrimmingArea
        {
            public List<Point> Polygon { get; set; } = new List<Point>();
        }
    }
}