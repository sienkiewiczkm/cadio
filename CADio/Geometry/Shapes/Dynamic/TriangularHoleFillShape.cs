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
using CADio.Views.ShapeEditors;

namespace CADio.Geometry.Shapes.Dynamic
{
    public class TriangularHoleFillShape : IDynamicShape
    {
        public string Name { get; } = "Triangular Hole Fill";
        public IList<Vertex> Vertices { get; set; } = new List<Vertex>();
        public IList<IndexedLine> Lines { get; set; } = new List<IndexedLine>();
        public IList<Vertex> MarkerPoints { get; set; } = new List<Vertex>();
        public IList<Vertex> RawPoints { get; set; } = new List<Vertex>();

        public Control GetEditorControl() => new TriangularFillEditor()
        {
            DataContext = this,
        };

        public IList<BezierSurfaceWorldObject> ReferenceSurfaces { get; set; }
        public IList<VirtualPoint> ReferencePoints { get; set; } =
            new List<VirtualPoint>();

        public IList<IParametricSurface> EdgesUParametrisations { get; set; }
            = new List<IParametricSurface>();

        public double Distance { get; set; } = 0.1f;
        public bool DrawVectors { get; set; } = false;

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
                )*Distance;
             
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

            /*
            for (var i = 0; i < 3; ++i)
            {
                for (var j = 0; j < 3; ++j)
                    builder.Connect(innerControlPoints[i, j]);
                builder.Connect(innerCurvesMiddlePoint);
                builder.FinishChain();
            }
            */

            for (var i = 0; i < EdgesUParametrisations.Count; ++i)
            {
                var currentMiddleCurve = i;
                var previousMiddleCurve = (currentMiddleCurve + 2)%3;
                var nextMiddleCurve = (currentMiddleCurve + 1)%3;

                var currentSurface = EdgesUParametrisations[i];
                var nextSurface = EdgesUParametrisations[(i + 1) % 3];

                var nextG0 = -nextSurface.Derivative(
                    0.0, 0.0, DerivativeParameter.V
                );

                var nextG2 = -nextSurface.Derivative(
                    0.5, 0.0, DerivativeParameter.V
                );

                var currentG0 = -currentSurface.Derivative(
                    0.5, 0.0, DerivativeParameter.V
                );

                var currentG2 = -currentSurface.Derivative(
                    1.0, 0.0, DerivativeParameter.V
                );

                var nextMiddleBoundaryDerivative = 
                    nextSurface.Derivative(0.5, 0.0, DerivativeParameter.U);

                var currentMiddleBoundaryDerivative = 
                    currentSurface.Derivative(0.5, 0.0, DerivativeParameter.U);

                var curveMidNextControlPoints = new List<Point3D>();
                var curveMidCurrentControlPoints = new List<Point3D>();

                for (var j = 0; j < 3; ++j)
                {
                    curveMidNextControlPoints.Add(
                        innerControlPoints[nextMiddleCurve, j]);
                    curveMidCurrentControlPoints.Add(
                        innerControlPoints[currentMiddleCurve, j]);
                }

                curveMidNextControlPoints.Add(innerCurvesMiddlePoint);
                curveMidCurrentControlPoints.Add(innerCurvesMiddlePoint);

                var curveMidNextDerivative = 
                    BernsteinPolynomial.CalculateDerivative(
                        curveMidNextControlPoints
                    );

                var curveMidCurrentDerivative =
                    BernsteinPolynomial.CalculateDerivative(
                        curveMidCurrentControlPoints
                    );

                var currentIn = 3*(innerCurvesMiddlePoint -
                    innerControlPoints[previousMiddleCurve, 2]);
                var currentOut = 3*(innerControlPoints[nextMiddleCurve, 2] -
                    innerCurvesMiddlePoint);

                var previousIn = 3*(innerCurvesMiddlePoint - 
                    innerControlPoints[currentMiddleCurve, 2]);
                var previousOut = 3*(innerControlPoints[previousMiddleCurve, 2] -
                    innerCurvesMiddlePoint);

                var gregoryPatch = new GregoryPatch();

                gregoryPatch.ReshapeEdge(
                    0, 
                    (t) => nextSurface.Evaluate(0.5*t, 0),
                    (t) => 0.5*nextSurface.Derivative(
                        0.5*t, 0, DerivativeParameter.U),
                    nextG0, 
                    nextG0, 
                    nextG2, 
                    nextG2
                );

                gregoryPatch.ReshapeEdge(
                    1,
                    (t) => currentSurface.Evaluate(0.5+0.5*t, 0),
                    (t) => -0.5*currentSurface.Derivative(
                        0.5+0.5*t, 0, DerivativeParameter.U),
                    currentG0, 
                    currentG0, 
                    currentG2, 
                    currentG2
                );

                gregoryPatch.ReshapeEdge(
                    2,
                    (t) => BernsteinPolynomial.Evaluate3DPolynomial(
                            curveMidNextControlPoints, t),
                    (t) => (Vector3D) BernsteinPolynomial.Evaluate3DPolynomial(
                            curveMidNextDerivative, t),
                    nextMiddleBoundaryDerivative, 
                    nextMiddleBoundaryDerivative,
                    previousIn,
                    previousOut
                );

                gregoryPatch.ReshapeEdge(
                    3, 
                    (t) => BernsteinPolynomial.Evaluate3DPolynomial(
                            curveMidCurrentControlPoints, t), 
                    (t) => (Vector3D) BernsteinPolynomial.Evaluate3DPolynomial(
                            curveMidCurrentDerivative, t),
                    currentMiddleBoundaryDerivative,
                    currentMiddleBoundaryDerivative,
                    currentIn,
                    currentOut
                );


                sampler.Build(gregoryPatch);
                if (DrawVectors)
                {
                    GregoryPatchShape.DrawGregoryPatchDebugData(
                        gregoryPatch,
                        builder);
                }
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