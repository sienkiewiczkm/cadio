using System.Windows.Media.Media3D;

namespace CADio.Mathematics.Interfaces
{
    public enum DerivativeParameter
    {
        U,
        V,
    }

    public struct Parametrisation
    {
        public double U;
        public double V;

        public Parametrisation(double u, double v)
        {
            U = u;
            V = v;
        }

        public static Parametrisation operator +(
            Parametrisation current,
            Parametrisation other
            )
        {
            return new Parametrisation(
                current.U + other.U,
                current.V + other.V
            );
        }

        public static Parametrisation operator -(
            Parametrisation current, 
            Parametrisation other
            )
        {
            return new Parametrisation(
                current.U - other.U, 
                current.V - other.V
            );
        }
    };

    public interface IParametricSurface
    {
        Point3D Evaluate(double u, double v);
        Vector3D Derivative(double u, double v, DerivativeParameter parameter);
    }

    public static class ParametricSurfaceExtensions
    {
        public static Point3D Evaluate(
            this IParametricSurface surface,
            Parametrisation parametrisation
            )
        {
            return surface.Evaluate(
                parametrisation.U, 
                parametrisation.V
            );
        }

        public static Vector3D Derivative(
            this IParametricSurface surface,
            Parametrisation parametrisation,
            DerivativeParameter parameter
            )
        {
            return surface.Derivative(
                parametrisation.U, 
                parametrisation.V,
                parameter
            );
        }

        public static Vector3D Normal(
            this IParametricSurface surface,
            Parametrisation parametrisation)
        {
            var du = surface.Derivative(
                parametrisation,
                DerivativeParameter.U
            );

            var dv = surface.Derivative(
                parametrisation,
                DerivativeParameter.V
            );

            return Vector3D.CrossProduct(du, dv);
        }
    }
}