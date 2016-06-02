using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes.Builders;
using CADio.Mathematics.Numerical;
using CADio.Mathematics.Patches;
using CADio.SceneManagement.Surfaces;

namespace CADio.Geometry.Shapes.Dynamic
{
    public class GregoryPatchShape : IDynamicShape
    {
        public string Name { get; }
        public IList<Vertex> Vertices { get; set; } = new List<Vertex>();
        public IList<IndexedLine> Lines { get; set; } = new List<IndexedLine>();
        public IList<Vertex> MarkerPoints { get; set; } = new List<Vertex>();
        public IList<Vertex> RawPoints { get; set; } = new List<Vertex>();
        public Control GetEditorControl() => null;

        public BezierSurfaceWorldObject ObservedBezierDebug { get; set; }

        public void UpdateGeometry(
            Func<Point3D, Point3D, double> estimateScreenDistanceWithoutClip, 
            Predicate<Point3D> isInsideProjectiveCubePredicate
            )
        {
            var gregoryPatch = new GregoryPatch();

            // Setup blank
            gregoryPatch.SetCornerPoint(0, new Point3D(-1.0f, 0.0f, +1.0f));
            gregoryPatch.SetCornerPoint(1, new Point3D(+1.0f, 0.0f, +1.0f));
            gregoryPatch.SetCornerPoint(2, new Point3D(-1.0f, 0.0f, -1.0f));
            gregoryPatch.SetCornerPoint(3, new Point3D(+1.0f, 0.0f, -1.0f));

            gregoryPatch.SetCornerDerivatives(
                0, new Vector3D(0, 0, -1), new Vector3D(1, 0, 0)
            );

            gregoryPatch.SetCornerDerivatives(
                1, new Vector3D(0, 1, -1), new Vector3D(1, 0, 0)
            );

            gregoryPatch.SetCornerDerivatives(
                2, new Vector3D(0, 0, 1), new Vector3D(1, 0, 0)
            );

            gregoryPatch.SetCornerDerivatives(
                3, new Vector3D(0, 0, -1), new Vector3D(1, 0, 0)
            );

            gregoryPatch.SetCornerTwistVectors(
                0, new Vector3D(1, 0, 0), new Vector3D(0, 0, 1)
            );

            gregoryPatch.SetCornerTwistVectors(
                1, new Vector3D(-1, 0, 0), new Vector3D(0, 0, -1)
            );

            gregoryPatch.SetCornerTwistVectors(
                2, new Vector3D(1, 0, 0), new Vector3D(0, 0, 1)
            );

            gregoryPatch.SetCornerTwistVectors(
                3, new Vector3D(-1, 0, 0), new Vector3D(0, 0, 1)
            );

            // create tangent field d(v)
            var g2 = 3 * 
                (ObservedBezierDebug.GetVirtualPoint(3, 3).WorldPosition -
                ObservedBezierDebug.GetVirtualPoint(2, 3).WorldPosition); 

            var g0 = 3 * 
                (ObservedBezierDebug.GetVirtualPoint(3, 0).WorldPosition -
                ObservedBezierDebug.GetVirtualPoint(2, 0).WorldPosition);

            var observedEdge = new List<Point3D>()
            {
                ObservedBezierDebug.GetVirtualPoint(3, 0).WorldPosition,
                ObservedBezierDebug.GetVirtualPoint(3, 1).WorldPosition,
                ObservedBezierDebug.GetVirtualPoint(3, 2).WorldPosition,
                ObservedBezierDebug.GetVirtualPoint(3, 3).WorldPosition,
            };

            gregoryPatch.ReshapeEdge(3, observedEdge, g0, g0, g2, g2);

            /*
            var g1 = (g0+g2)/2.0;

            var polynomialCoords = new List<Point3D>
            {
                (Point3D) g0,
                (Point3D) g1,
                (Point3D) g2
            };

            var twist1 = (Vector3D)BernsteinPolynomial.Evaluate3DPolynomial(
                polynomialCoords, 1.0/3
            );

            var twist2 = (Vector3D)BernsteinPolynomial.Evaluate3DPolynomial(
                polynomialCoords, 2.0/3
            );

            // apply test bezier on one side
            gregoryPatch.SetCornerPoint(0,
                ObservedBezierDebug.GetVirtualPoint(3, 3).WorldPosition);
            gregoryPatch.SetCornerPoint(2,
                ObservedBezierDebug.GetVirtualPoint(3, 0).WorldPosition);

            gregoryPatch.SetCornerDerivatives(0, 
                new Vector3D(0,0,-1), g0);
            gregoryPatch.SetCornerDerivatives(2, 
                new Vector3D(0,0,-1), g2);

            gregoryPatch.SetCornerTwistVectors(0,
                twist1, new Vector3D(0, 0, -1));
            gregoryPatch.SetCornerTwistVectors(2,
                twist2, new Vector3D(0, 0, 1));
            */

            var builder = new WireframeBuilder();
            var surfaceSampler = new SurfaceConstantParameterLinesBuilder(
                builder
            );

            surfaceSampler.Build(gregoryPatch);
            DrawGregoryPatchDebugData(gregoryPatch, builder);

            Vertices = builder.Vertices.ToList();
            Lines = builder.Lines.ToList();
        }

        public static void DrawGregoryPatchDebugData(
            GregoryPatch gregoryPatch, 
            WireframeBuilder builder
            )
        {
            const double cVectorMagnitude = 0.5;
            for (var i = 0; i < 4; ++i)
            {
                var cornerPos = gregoryPatch.GetCornerPoint(i);
                var cornerdU = gregoryPatch.GetCornerDerivativeU(i);
                var cornerdV = gregoryPatch.GetCornerDerivativeV(i);
                builder.DrawVector(cornerPos, cornerdU, cVectorMagnitude,
                    Colors.DodgerBlue);
                builder.DrawVector(cornerPos, cornerdV, cVectorMagnitude,
                    Colors.Gold);

                cornerdU.Normalize();
                cornerdV.Normalize();

                var uEnd = cornerPos + cVectorMagnitude * cornerdU;
                var vEnd = cornerPos + cVectorMagnitude * cornerdV;
                var uvTwist = gregoryPatch.GetCornerTwistVectorUV(i);
                var vuTwist = gregoryPatch.GetCornerTwistVectorVU(i);

                builder.DrawVector(
                    uEnd, uvTwist, cVectorMagnitude, Colors.Chartreuse
                );

                builder.DrawVector(
                    vEnd, vuTwist, cVectorMagnitude, Colors.Chartreuse
                );
            }
        }
    }
}