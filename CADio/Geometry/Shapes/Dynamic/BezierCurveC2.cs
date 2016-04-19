using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.Mathematics;
using CADio.Mathematics.Numerical;
using CADio.Views.ShapeEditors;

namespace CADio.Geometry.Shapes.Dynamic
{
    public enum CurveBasis
    {
        [Description("Bernstein")]
        Bernstein,

        [Description("B-Spline")]
        BSpline,
    }

    public class BezierCurveC2 : IDynamicShape
    {
        public string Name => "Bezier Curve C2";

        public IList<Vertex> Vertices { get; private set; }
        public IList<IndexedLine> Lines { get; private set; }
        public IList<Vertex> MarkerPoints { get; private set; } = new List<Vertex>();
        public IList<Vertex> RawPoints { get; private set; }
        public bool IsPolygonRenderingEnabled { get; set; }

        public Control GetEditorControl() => new BezierCurveC2Editor() {DataContext = this};

        public IList<Point3D> ControlPoints = new List<Point3D>();
        public IList<double> CustomKnots;
        public bool ApplyParameterCorrection { get; set; } = true;

        public CurveBasis Basis { get; set; } = CurveBasis.BSpline;

        public void UpdateGeometry(Func<Point3D, Point3D, double> estimateScreenSpaceDistanceWithoutClip,
            Predicate<Point3D> isInsideProjectiveCubePredicate)
        {
            IList<Point3D> chosenControlPoints;
            var controlPointsColor = Colors.White;
            IList<Vertex> rawPointsList = new List<Vertex>();

            switch (Basis)
            {
                case CurveBasis.Bernstein:
                    chosenControlPoints = BSplineToBernsteinConverter.ConvertAssumingEquidistantKnots(
                        ControlPoints
                    );

                    rawPointsList = BezierCurveC0.SampleBezierCurveC0(
                        chosenControlPoints,
                        estimateScreenSpaceDistanceWithoutClip, 
                        3
                    );

                    controlPointsColor = Colors.Red;
                    break;
                case CurveBasis.BSpline:
                    chosenControlPoints = ControlPoints;
                    rawPointsList = SampleBSplineCurve(ControlPoints, CustomKnots, ApplyParameterCorrection, estimateScreenSpaceDistanceWithoutClip);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            RawPoints = new List<Vertex>();
            Vertices = new List<Vertex>();
            MarkerPoints = new List<Vertex>();
            Lines = new List<IndexedLine>();

            if (rawPointsList.Count > 0)
            {
                Vertices = rawPointsList;
                Lines = Enumerable.Range(0, rawPointsList.Count - 1).Select(t => new IndexedLine(t, t + 1)).ToList();
            }

            if (IsPolygonRenderingEnabled && ControlPoints.Count >= 2)
            {
                var additionalVertices = ControlPoints.Select(t => new Vertex(t, Colors.Gray)).ToList();
                var additionalLines = Enumerable.Range(Vertices.Count, ControlPoints.Count - 1).Select(t => new IndexedLine(t, t + 1)).ToList();

                Vertices = Vertices.Concat(additionalVertices).ToList();
                Lines = Lines.Concat(additionalLines).ToList();
            }

            if (!ReferenceEquals(chosenControlPoints, ControlPoints))
                MarkerPoints = chosenControlPoints.Select(t => new Vertex(t, controlPointsColor)).ToList();

            // display bernstein points
            /*
            MarkerPoints = new List<Vertex>();
            if (ControlPoints.Count > 3)
            {
                var bernsteinVertices =
                    BSplineToBernsteinConverter.ConvertAssumingEquidistantKnots(ControlPoints);
                MarkerPoints = bernsteinVertices
            }
            */
            // *** 
        }

        public static List<Vertex> SampleBSplineCurve(IList<Point3D> controlPoints, IList<double> knots, bool parameterCorrection,
            Func<Point3D, Point3D, double> estimateScreenSpaceDistanceWithoutClip)
        {
            var rawPointsList = new List<Vertex>();
            var solver = new DeBoorSolver3D(controlPoints, knots);

            if (controlPoints.Count <= 3)
                return rawPointsList;

            var bernsteinPolygonLengthNoClip = BezierCurveC0.EstimateScreenSpacePolygonLength(
                controlPoints, 
                estimateScreenSpaceDistanceWithoutClip, 
                controlPoints.Count - 1, 
                0
            );

            var generatedPointsBetween = (int)Math.Ceiling(bernsteinPolygonLengthNoClip);

            for (var j = 0; j < generatedPointsBetween; ++j)
            {
                var t = (double) j/generatedPointsBetween;
                var lerped = solver.Evaluate(t, parameterCorrection);
                rawPointsList.Add(new Vertex() {Position = lerped, Color = Colors.White});
            }

            return rawPointsList;
        }
    }
}