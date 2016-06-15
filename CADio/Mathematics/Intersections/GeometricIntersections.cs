using System;
using System.Windows;
using MathNet.Numerics.LinearAlgebra;

namespace CADio.Mathematics.Intersections
{
    public enum SegmentIntersectionKind
    {
        None,
        Internal,
        Beginning,
        Ending,
        Whole
    }

    public static class GeometricIntersections
    {
        public const double ZeroEpsilon = 0.00001;

        public static bool AlmostZero(double value)
        {
            return Math.Abs(value) <= ZeroEpsilon;
        }

        public static bool IsPositive(double value)
        {
            return value > ZeroEpsilon;
        }

        public static bool IsNegative(double value)
        {
            return value < -ZeroEpsilon;
        }

        public static bool SameCrossSign(
            Point origin,
            Point middlePoint,
            Point firstEnd,
            Point secondEnd
            )
        {
            var baseVector = middlePoint - origin;
            var firstVector = firstEnd - middlePoint;
            var secondVector = secondEnd - middlePoint;

            var firstCross = Vector.CrossProduct(baseVector, firstVector);
            var secondCross = Vector.CrossProduct(baseVector, secondVector);

            if (IsPositive(firstCross) && IsPositive(secondCross))
                return true;

            if (IsNegative(firstCross) && IsNegative(secondCross))
                return true;

            return false;
        }

        public static SegmentIntersectionKind CheckRayLineSegmentCollision(
            Point rayOrigin,
            Point rayPoint,
            Point segmentBeginning,
            Point segmentEnd
            )
        {
            var rayVector = rayPoint - rayOrigin;
            var rayOriginToBeginning = segmentBeginning - rayOrigin;
            var rayOriginToEnd = segmentEnd - rayOrigin;
            var rayTipToBeginning = segmentBeginning - rayPoint;
            var rayTipToEnd = segmentEnd - rayPoint;

            var crossToBeginning = Vector.CrossProduct(
                rayVector,
                rayTipToBeginning
            );

            var crossToEnd = Vector.CrossProduct(
                rayVector,
                rayTipToEnd
            );

            if ((IsPositive(crossToBeginning) && IsPositive(crossToEnd)) ||
                (IsNegative(crossToBeginning) && IsNegative(crossToEnd)))
                return SegmentIntersectionKind.None;

            if ((AlmostZero(crossToBeginning) && AlmostZero(crossToEnd)))
                return SegmentIntersectionKind.Whole;

            var dot1 = rayVector.X*rayOriginToBeginning.X +
                rayVector.Y*rayOriginToBeginning.Y;

            var dot2 = rayVector.X*rayOriginToEnd.X +
                rayVector.Y*rayOriginToEnd.Y;

            if (AlmostZero(crossToBeginning) && IsPositive(dot1))
                return SegmentIntersectionKind.Beginning;
            
            if (AlmostZero(crossToEnd) && IsPositive(dot2))
                return SegmentIntersectionKind.Ending;

            if (IsPositive(dot1) || IsPositive(dot2))
                if (!SameCrossSign(
                        segmentBeginning,
                        segmentEnd,
                        rayOrigin,
                        rayPoint
                    ))
                {
                    return SegmentIntersectionKind.Internal;
                }

            return SegmentIntersectionKind.None;
        }   
    }
}