using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes.Dynamic;
using CADio.Mathematics.Interfaces;
using CADio.Mathematics.Patches;

namespace CADio.Mathematics.Surfaces
{
    public class BezierSurface :
        IParametricSurface
    {
        public int SegmentsU { get; set; }
        public int SegmentsV { get; set; }
        public List<Point3D> ControlPoints { get; set; }

        public Point3D Evaluate(
            double u, 
            double v
            )
        {
            double patchU, patchV;

            var patch = GetPatchWithAdjustedCoords(
                u, 
                v, 
                out patchU, 
                out patchV
            );

            return patch.Evaluate(patchU, patchV);
        }

        public Vector3D Derivative(
            double u, 
            double v, 
            DerivativeParameter parameter
            )
        {
            double patchU, patchV;

            var patch = GetPatchWithAdjustedCoords(
                u, 
                v, 
                out patchU, 
                out patchV
            );

            double innerDerivative;
            switch (parameter)
            {
                case DerivativeParameter.U:
                    innerDerivative = SegmentsU;
                    break;
                case DerivativeParameter.V:
                    innerDerivative = SegmentsV;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(parameter), 
                        parameter, 
                        null
                    );
            }

            return innerDerivative*patch.Derivative(patchU, patchV, parameter);
        }

        private BernsteinPatch GetPatchWithAdjustedCoords(double u, double v, out double adjustedU, out double adjustedV)
        {
            var du = 1.0/SegmentsU;
            var dv = 1.0/SegmentsV;
            var uPatchNum = (int) Math.Min(SegmentsU - 1, u/du);
            var vPatchNum = (int) Math.Min(SegmentsV - 1, v/dv);
            adjustedU = (u - du*uPatchNum)/du;
            adjustedV = (v - dv*vPatchNum)/dv;
            return GetPatchByLocation(uPatchNum, vPatchNum);
        }

        private BernsteinPatch GetPatchByLocation(int uPatchNum, int vPatchNum)
        {
            var patchControlPoints = BezierPatchGroup.GetList2DSubRectCyclic(ControlPoints, 3*SegmentsU + 1, 3*uPatchNum, 3*vPatchNum, 4, 4);

            var bezierPatch = new BernsteinPatch();
            for (var i = 0; i < 16; ++i)
                bezierPatch.ControlPoints[i/4, i%4] = patchControlPoints[i];

            return bezierPatch;
        }
    }
}