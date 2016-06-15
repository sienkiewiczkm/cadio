using System.Windows.Media.Media3D;
using CADio.Mathematics.Interfaces;

namespace CADio.Mathematics.Proxies
{
    public class SurfaceReparametrisationFlip : IParametricSurface
    {
        public IParametricSurface Surface { get; set; }
        public bool FlipU { get; set; }
        public bool FlipV { get; set; }
        public bool FlipUV { get; set; }

        public ParametrisationBoundaries ParametrisationBoundaries
            => Surface.ParametrisationBoundaries;

        public Point3D Evaluate(double u, double v)
        {
            AdjustCoorinates(ref u, ref v);
            return Surface.Evaluate(u, v);
        }

        public Vector3D Derivative(
            double u,
            double v,
            DerivativeParameter parameter)
        {
            AdjustCoorinates(ref u, ref v);
            AdjustDerivativeParameter(ref parameter);

            var flip = (FlipU && parameter == DerivativeParameter.U)
                || (FlipV && parameter == DerivativeParameter.V);
            var flipFactor = flip ? -1.0 : 1.0;

            return flipFactor*Surface.Derivative(u, v, parameter);
        }

        private void AdjustCoorinates(ref double u, ref double v)
        {
            if (FlipUV)
            {
                var savedU = u;
                u = v;
                v = savedU;
            }

            u = FlipU ? 1 - u : u;
            v = FlipV ? 1 - v : v;
        }

        private void AdjustDerivativeParameter(ref DerivativeParameter parameter)
        {
            if (FlipUV)
                parameter = parameter == DerivativeParameter.U
                    ? DerivativeParameter.V
                    : DerivativeParameter.U;
        }
    }
}