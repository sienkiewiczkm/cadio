using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes.Builders;
using CADio.Mathematics;
using CADio.Mathematics.Interfaces;
using CADio.Mathematics.Numerical;
using CADio.Mathematics.Patches;
using CADio.SceneManagement;
using CADio.SceneManagement.Points;
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

        public IList<IParametricSurface> EdgesUParametrisations { get; set; }
            = new List<IParametricSurface>();

        public void UpdateGeometry(
            Func<Point3D, Point3D, double> estimateScreenDistanceWithoutClip,
            Predicate<Point3D> isInsideProjectiveCubePredicate
            )
        {
            CreateSurfacesReparametrisations();
            Debug.Assert(EdgesUParametrisations.Count == 3);

            var builder = new WireframeBuilder();
            var sampler = new SurfaceConstantParameterLinesBuilder(builder);

            var innerControlPoints = new Point3D[3, 3];
            for (var i = 0; i < 3; ++i)
                innerControlPoints[i, 0] = EdgesUParametrisations[i].Evaluate(
                    0.5, 0.0
                );

            for (var i = 0; i < 3; ++i)
            {
                var outwardDerivativeV = -EdgesUParametrisations[i].Derivative(
                    0.5,
                    0.0,
                    DerivativeParameter.V
                );
             
                innerControlPoints[i, 1] = innerControlPoints[i, 0] 
                    + outwardDerivativeV;
            }

            var auxiliaryPoints = new Point3D[3];
            var auxiliarySum = new Vector3D();
            for (var i = 0; i < 3; ++i)
            {
                auxiliaryPoints[i] =
                    innerControlPoints[i, 0] +
                    3.0*(innerControlPoints[i, 1]
                         - innerControlPoints[i, 0])/2.0;
                auxiliarySum += (Vector3D)auxiliaryPoints[i];
            }

            var innerCurvesMiddlePoint = (Point3D) (auxiliarySum/3.0);

            for (var i = 0; i < 3; ++i)
            {
                innerControlPoints[i, 2] = innerCurvesMiddlePoint +
                    2.0*(auxiliaryPoints[i] - innerCurvesMiddlePoint)/3.0;
            }

            //for (var i = 0; i < 3; ++i)
            //{
            //    for (var j = 0; j < 3; ++j)
            //        builder.Connect(innerControlPoints[i, j]);
            //    builder.Connect(innerCurvesMiddlePoint);
            //    builder.FinishChain();
            //}

            //foreach (var currentSurface in EdgesUParametrisations)
            for (var i = 0; i < EdgesUParametrisations.Count; ++i)
            {
                var currentMiddleCurve = i;
                var previousMiddleCurve = (currentMiddleCurve + 2)%3;
                var nextMiddleCurve = (currentMiddleCurve + 1)%3;

                var currentSurface = EdgesUParametrisations[i];
                var nextSurface = EdgesUParametrisations[(i + 1) % 3];

                var nextG0 = nextSurface.Derivative(
                    0.0, 0.0, DerivativeParameter.V
                );

                var nextG2 = nextSurface.Derivative(
                    0.5, 0.0, DerivativeParameter.V
                );

                var currentG0 = currentSurface.Derivative(
                    0.5, 0.0, DerivativeParameter.V
                );

                var currentG2 = currentSurface.Derivative(
                    1.0, 0.0, DerivativeParameter.V
                );

                builder.DrawDerivative(
                    currentSurface,
                    0.0,
                    0.0,
                    DerivativeParameter.U,
                    Colors.Fuchsia);

                builder.DrawDerivative(
                    currentSurface,
                    0.5,
                    0.0,
                    DerivativeParameter.U,
                    Colors.Cyan);

                var nextMiddleBoundaryDerivative = 
                    nextSurface.Derivative(0.5, 0.0, DerivativeParameter.V);

                var currentMiddleBoundaryDerivative = currentSurface.Derivative(
                    0.5, 0.0, DerivativeParameter.V
                );

                var curveToCenterControlPoints = new List<Point3D>();
                var curveFromCenterControlPoints = new List<Point3D>
                {
                    innerCurvesMiddlePoint
                };

                for (var j = 0; j < 3; ++j)
                {
                    curveToCenterControlPoints.Add(
                        innerControlPoints[nextMiddleCurve, j]);
                    curveFromCenterControlPoints.Add(
                        innerControlPoints[currentMiddleCurve, 2 - j]);
                }

                curveToCenterControlPoints.Add(innerCurvesMiddlePoint);

                var curveToCenterDerivative = 
                    BernsteinPolynomial.CalculateDerivative(
                        curveToCenterControlPoints
                    );

                var curveFromCenterDerivative =
                    BernsteinPolynomial.CalculateDerivative(
                        curveFromCenterControlPoints
                    );

                var currentA2 = (innerControlPoints[previousMiddleCurve, 2] -
                    innerCurvesMiddlePoint);
                var currentB2 = (innerCurvesMiddlePoint -
                    innerControlPoints[nextMiddleCurve, 2]);

                var previousA2 = (innerControlPoints[nextMiddleCurve, 2] -
                    innerCurvesMiddlePoint);
                var previousB2 = (innerCurvesMiddlePoint - 
                    innerControlPoints[currentMiddleCurve, 2]);

                var gregoryPatch = new GregoryPatch();

                gregoryPatch.ReshapeEdge(
                    0, 
                    (t) => nextSurface.Evaluate(0.5*t, 0),
                    (t) => nextSurface.Derivative(
                        0.5*t, 0, DerivativeParameter.U),
                    nextG0, 
                    nextG0, 
                    nextG2, 
                    nextG2
                );

                gregoryPatch.ReshapeEdge(
                    1,
                    (t) => currentSurface.Evaluate(0.5+0.5*t, 0),
                    (t) => -currentSurface.Derivative(
                        0.5+0.5*t, 0, DerivativeParameter.U),
                    currentG0, 
                    currentG0, 
                    currentG2, 
                    currentG2
                );

                gregoryPatch.ReshapeEdge(
                    2,
                    (t) => BernsteinPolynomial.Evaluate3DPolynomial(
                            curveToCenterControlPoints, t),
                    (t) => (Vector3D) BernsteinPolynomial.Evaluate3DPolynomial(
                            curveToCenterDerivative, t),
                    currentMiddleBoundaryDerivative,
                    currentMiddleBoundaryDerivative,
                    currentA2,
                    currentB2
                );

                gregoryPatch.ReshapeEdge(
                    3, 
                    (t) => BernsteinPolynomial.Evaluate3DPolynomial(
                            curveFromCenterControlPoints, t), 
                    (t) => (Vector3D) BernsteinPolynomial.Evaluate3DPolynomial(
                            curveFromCenterDerivative, t),
                    previousA2, 
                    previousB2,
                    nextMiddleBoundaryDerivative, 
                    nextMiddleBoundaryDerivative
                );


                sampler.Build(gregoryPatch);
                GregoryPatchShape.DrawGregoryPatchDebugData(gregoryPatch,
                    builder);

                break;
            }

            Vertices = builder.Vertices.ToList();
            Lines = builder.Lines.ToList();
        }

        private void CreateSurfacesReparametrisations()
        {
            EdgesUParametrisations.Clear();
            // todo: move this responsibility somewhere else
            for (var i = 0; i < ReferenceSurfaces.Count; ++i)
            {
                var startPoint = ReferencePoints[3*i];
                var endPoint =
                    ReferencePoints[(3*(i + 1))%ReferencePoints.Count];
                var surface = ReferenceSurfaces[i];
                var startCoordinate = 
                    surface.GetVirtualPointCoordinate(startPoint);
                var endCoordinate = 
                    surface.GetVirtualPointCoordinate(endPoint);

                EdgesUParametrisations.Add(
                    surface.GetBernsteinPatch()
                        .CreateReparametrisationUDirection(
                            startCoordinate.Item1,
                            startCoordinate.Item2,
                            endCoordinate.Item1,
                            endCoordinate.Item2
                         )
                );
            }
        }
    }
}