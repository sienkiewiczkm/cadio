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
            throw new NotImplementedException();
        }

        public static void DrawGregoryPatchDebugData(
            GregoryPatch gregoryPatch, 
            WireframeBuilder builder
            )
        {
            const double cVectorMagnitude = 0.1;
            var uParams = new float[] {0, 0, 1, 1};
            var vParams = new float[] {0, 1, 0, 1};
            var uFlips = new float[] {1, 1, -1, -1};
            var vFlips = new float[] {1, -1, 1, -1};
            var neighbours = new int[,]
            {
                {2, 1},
                {3, 0},
                {0, 3},
                {1, 2},
            };

            for (var i = 0; i < 4; ++i)
            {
                var cornerPos = gregoryPatch.Evaluate(uParams[i], vParams[i]);
                var cornerdU = gregoryPatch.GetCornerDerivativeU(i);
                var cornerdV = gregoryPatch.GetCornerDerivativeV(i);

                var uvTwistPos = gregoryPatch.Evaluate(
                    (2*uParams[i] + uParams[neighbours[i, 0]])/3.0,
                    (2*vParams[i] + vParams[neighbours[i, 0]])/3.0
                );

                var vuTwistPos = gregoryPatch.Evaluate(
                    (2*uParams[i] + uParams[neighbours[i, 1]])/3.0,
                    (2*vParams[i] + vParams[neighbours[i, 1]])/3.0
                );

                var uvTwist = gregoryPatch.GetCornerTwistVectorUV(i);
                var vuTwist = gregoryPatch.GetCornerTwistVectorVU(i);

                builder.DrawVector(
                    cornerPos, 
                    cornerdU*uFlips[i], 
                    cVectorMagnitude,
                    Colors.Red
                );

                builder.DrawVector(
                    cornerPos, 
                    cornerdV*vFlips[i], 
                    cVectorMagnitude,
                    Colors.Blue
                );

                builder.DrawVector(
                    uvTwistPos, 
                    uvTwist, 
                    cVectorMagnitude,
                    Colors.GreenYellow
                );

                builder.DrawVector(
                    vuTwistPos, 
                    vuTwist, 
                    cVectorMagnitude,
                    Colors.Magenta
                );
            }
        }
    }
}