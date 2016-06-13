using System;
using System.Windows.Media.Media3D;
using CADio.Mathematics.Interfaces;

namespace CADio.Mathematics.Surfaces
{
    public class SurfaceSamplingNearestFinder :
        ISurfaceNearestPointFinder
    {
        private int _samplesU;
        private int _samplesV;

        public int SamplesU
        {
            get { return _samplesU; }
            set { _samplesU = Math.Max(2, value); }
        }

        public int SamplesV
        {
            get { return _samplesV; }
            set { _samplesV = Math.Max(2, value); }
        }

        public SurfaceSamplingNearestFinder()
        {
            SamplesU = 4;
            SamplesV = 4;
        }

        public Parametrisation FindNearest(
            IParametricSurface surface,
            Point3D referencePoint
            )
        {
            Parametrisation closestParametrisation = new Parametrisation();
            var closestDistance = double.MaxValue;

            for (var u = 0; u < SamplesU; ++u)
            {
                for (var v = 0; v < SamplesV; ++v)
                {
                    var paramU = u/(SamplesU - 1.0);
                    var paramV = v/(SamplesV - 1.0);
                    var location = surface.Evaluate(paramU, paramV);
                    var distance = (referencePoint - location).Length;
                    if (closestDistance > distance)
                    {
                        closestDistance = distance;
                        closestParametrisation = new Parametrisation(
                            paramU,
                            paramV
                        );
                    }
                }
            }

            return closestParametrisation;
        }
    }
}