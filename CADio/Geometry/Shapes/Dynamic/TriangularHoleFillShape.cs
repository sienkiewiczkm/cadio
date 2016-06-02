using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes.Builders;
using CADio.Mathematics;
using CADio.Mathematics.Numerical;
using CADio.Mathematics.Patches;
using CADio.SceneManagement;
using CADio.SceneManagement.Surfaces;

namespace CADio.Geometry.Shapes.Dynamic
{
    public class TriangularHoleFillShape : IDynamicShape
    {
        public string Name { get; }
        public IList<Vertex> Vertices { get; set; } = new List<Vertex>();
        public IList<IndexedLine> Lines { get; set; } = new List<IndexedLine>();
        public IList<Vertex> MarkerPoints { get; set; } = new List<Vertex>();
        public IList<Vertex> RawPoints { get; set; } = new List<Vertex>();
        public Control GetEditorControl() => null;

        public IList<BezierSurfaceWorldObject> ReferenceSurfaces { get; set; }
        public IList<VirtualPoint> ReferencePoints { get; set; } =
            new List<VirtualPoint>();

        public void UpdateGeometry(
            Func<Point3D, Point3D, double> estimateScreenDistanceWithoutClip,
            Predicate<Point3D> isInsideProjectiveCubePredicate
            )
        {
            var triangleMiddlePoints = new Point3D[3, 3];
            Point3D middlePoint = new Point3D();

            for (var i = 0; i < 3; ++i)
                triangleMiddlePoints[i, 0] = FindPointOnSideCurve(i);

            var builder = new WireframeBuilder();
            var sampler = new SurfaceConstantParameterLinesBuilder(builder);
            
            for (var i = 0; i < 3; ++i)
            {
                var deriv = GetDerivativeForCorrectSide(i)(0.5);
                builder.DrawVector(
                    triangleMiddlePoints[i, 0], deriv, deriv.Length
                );
                triangleMiddlePoints[i, 1] = triangleMiddlePoints[i, 0] + deriv;
            }

            var auxiliaryPoints = new Point3D[3];
            var auxiliarySum = new Vector3D();
            for (var i = 0; i < 3; ++i)
            {
                auxiliaryPoints[i] =
                    triangleMiddlePoints[i, 0] +
                    3.0*(triangleMiddlePoints[i, 1]
                         - triangleMiddlePoints[i, 0])/2.0;
                auxiliarySum += (Vector3D)auxiliaryPoints[i];
            }

            middlePoint = (Point3D) (auxiliarySum/3.0);

            for (var i = 0; i < 3; ++i)
            {
                triangleMiddlePoints[i, 2] = middlePoint +
                    2.0*(auxiliaryPoints[i] - middlePoint)/3.0;
            }

            var markers = new List<Point3D>();
            for (var i = 0; i < 3; ++i)
            {
                var currentSurf = i;
                var nextSurf = (i + 1)%3;
                var previousSurf = (i + 2)%3;

                var currentCurve = GetSideCurve(currentSurf);
                var currentCurveDerivative =
                    BernsteinPolynomial.CalculateDerivative(currentCurve);
                
                var previousCurve = GetSideCurve(previousSurf);
                var previousCurveDerivative =
                    BernsteinPolynomial.CalculateDerivative(previousCurve);

                var currentLeftSide = GetLeftPartOfSide(currentSurf);
                var previousRightSide = GetRightPartOfSide(previousSurf);
                var toCenter = new List<Point3D>();
                var fromCenter = new List<Point3D>();
                fromCenter.Add(middlePoint);
                for (var j = 0; j < 3; ++j)
                {
                    toCenter.Add(triangleMiddlePoints[currentSurf, j]);
                    fromCenter.Add(triangleMiddlePoints[previousSurf, 2 - j]);
                }
                toCenter.Add(middlePoint);

                markers.AddRange(currentLeftSide);
                markers.AddRange(previousRightSide);
                markers.AddRange(toCenter);
                markers.AddRange(fromCenter);

                var currentSurfDerivative =
                    GetDerivativeForCorrectSide(currentSurf);
                var previousSurfDerivative =
                    GetDerivativeForCorrectSide(previousSurf);

                var g0 = currentSurfDerivative(0);
                var g2 = currentSurfDerivative(0.5);
                var prevg0 = -previousSurfDerivative(0.5);
                var prevg2 = -previousSurfDerivative(1);

                var baseDeriv =
                    -(Vector3D)
                        BernsteinPolynomial.Evaluate3DPolynomial(currentCurveDerivative,
                            0.5);

                var prevDeriv =
                    (Vector3D)
                        BernsteinPolynomial.Evaluate3DPolynomial(previousCurveDerivative,
                            0.5);

                var currentA2 = (triangleMiddlePoints[previousSurf, 2] -
                                middlePoint);
                var currentB2 = (middlePoint - triangleMiddlePoints[nextSurf, 2]);

                var previousA2 = (triangleMiddlePoints[nextSurf, 2] -
                                middlePoint);
                var previousB2 = (middlePoint - triangleMiddlePoints[currentSurf, 2]);

                var gregoryPatch = new GregoryPatch();
                gregoryPatch.ReshapeEdge(0, toCenter, baseDeriv, baseDeriv,
                    currentA2, currentB2, -1);
                gregoryPatch.ReshapeEdge(1, fromCenter, previousA2, previousB2,
                    prevDeriv, prevDeriv, -1);
                gregoryPatch.ReshapeEdge(3, currentLeftSide, g0, g0, g2, g2);
                gregoryPatch.ReshapeEdge(2, previousRightSide, prevg0, prevg0, prevg2, prevg2);

                sampler.Build(gregoryPatch);
                GregoryPatchShape.DrawGregoryPatchDebugData(gregoryPatch,
                    builder);
            }

            for (var i = 0; i < 3; ++i)
            {
                for (var j = 0; j < 3; ++j)
                    builder.Connect(triangleMiddlePoints[i, j]);
                builder.Connect(middlePoint);
                builder.FinishChain();
            }

            Vertices = builder.Vertices.ToList();
            Lines = builder.Lines.ToList();

            MarkerPoints =
                markers.Select(t => new Vertex(new Point3D(t.X,t.Y,t.Z), 
                Colors.GreenYellow)).ToList();
        }

        public Point3D FindPointOnSideCurve(int curve)
        {
            var curvePoints = GetSideCurve(curve);

            var bernsteinArray =
                BezierCurveC0.FillBernsteinCoordinatesArray(curvePoints, 3, 0);
            var solver = new DeCastlejauSolver(bernsteinArray);
            return MathHelpers.MakePoint3D(solver.Evaluate(0.5));
        }

        private List<Point3D> GetSideCurve(int curve)
        {
            var curvePoints = new List<Point3D>();
            for (var i = 0; i < 4; ++i)
                curvePoints.Add(
                    ReferencePoints[(3*curve + i)%ReferencePoints.Count]
                        .Position
                    );
            return curvePoints;
        }

        public Func<double, Vector3D> GetDerivativeForCorrectSide(int surfaceId)
        {
            // todo : remove assumption
            var exclusiveVertex = ReferencePoints[surfaceId*3 + 1];
            var surf = ReferenceSurfaces[surfaceId];
            var coord = surf.GetVirtualPointCoordinate(
                exclusiveVertex
            );

            bool uDerivative = true;
            var negateFactor = 1.0;
            double u = 0.0, v = 0.0;

            if (coord.Item1 == 0) { u = 0.0; v = 0.5; negateFactor = -1.0; }
            else if (coord.Item1 == 3) { u = 1.0; v = 0.5; }
            else if (coord.Item2 == 0) { u = 0.5; v = 0.0;
                negateFactor = -1.0; uDerivative = false;}
            else if (coord.Item2 == 3) { u = 0.5; v = 1.0; uDerivative = false;}

            var patch = surf.GetBernsteinPatch();
            if (uDerivative)
                return x => negateFactor*patch.DerivativeU(u, x)/3.0;
            return x => negateFactor*patch.DerivativeV(x, v)/3.0;
        }

        public List<Point3D> GetLeftPartOfSide(int surfaceId)
        {
            var curve = GetSideCurve(surfaceId);
            var solver = new DeCastlejauSolver(
                BezierCurveC0.FillBernsteinCoordinatesArray(curve, 3, 0)
                );

            double[,] left, right;
            solver.EvaluateWithSubdivide(0.5, out left, out right);

            // todo : other dimensions than 3
            var leftcurve = new List<Point3D>();
            for (var i = 0; i < 4; i++)
                leftcurve.Add(new Point3D(left[i, 0], left[i, 1], left[i, 2]));
            return leftcurve;
        }

        public List<Point3D> GetRightPartOfSide(int surfaceId)
        {
            var curve = GetSideCurve(surfaceId);
            var solver = new DeCastlejauSolver(
                BezierCurveC0.FillBernsteinCoordinatesArray(curve, 3, 0)
                );

            double[,] left, right;
            solver.EvaluateWithSubdivide(0.5, out left, out right);

            // todo : other dimensions than 3
            var leftcurve = new List<Point3D>();
            for (var i = 0; i < 4; i++)
                leftcurve.Add(new Point3D(right[i, 0], right[i, 1], right[i, 2]));
            return leftcurve;
        }
    }
}