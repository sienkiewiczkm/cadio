using System;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using MathNet.Numerics.LinearAlgebra;

namespace CADio.Mathematics.Interfaces
{
    public enum DerivativeParameter
    {
        U,
        V,
    }

    public struct ParametrisationBoundaries
    {
        public bool AllowTraversalU;
        public bool AllowTraversalV;
    };

    public struct Parametrisation
    {
        public ParametrisationBoundaries Boundaries;

        public double UnboundedU;
        public double UnboundedV;

        public double U
        {
            get
            {
                if (Boundaries.AllowTraversalU)
                    return UnboundedU - Math.Floor(UnboundedU);
                return UnboundedU;
            }
            set { UnboundedU = value; }
        }

        public double V
        {
            get
            {
                if (Boundaries.AllowTraversalV)
                    return UnboundedV - Math.Floor(UnboundedV);
                return UnboundedV;
            }
            set { UnboundedV = value; }
        }

        public Parametrisation(double u, double v)
        {
            UnboundedU = u;
            UnboundedV = v;
            Boundaries = new ParametrisationBoundaries();
        }

        public Parametrisation(
            double u, 
            double v, 
            ParametrisationBoundaries boundaries)
        {
            UnboundedU = u;
            UnboundedV = v;
            Boundaries = boundaries;
        }

        public bool IsValid()
        {
            if (!Boundaries.AllowTraversalU &&
                (UnboundedU < 0.0 || UnboundedU > 1.0))
                return false;

            if (!Boundaries.AllowTraversalV &&
                (UnboundedV < 0.0 || UnboundedV > 1.0))
                return false;

            return true;
        }

        public override string ToString()
        {
            return
                $"U={U} V={V} UnboundedU={UnboundedU} UndboudedV={UnboundedV}";
        }

        public static Parametrisation operator +(
            Parametrisation left,
            Parametrisation right
            )
        {
            // todo: check boundaries
            return new Parametrisation(
                left.UnboundedU + right.UnboundedU,
                left.UnboundedV + right.UnboundedV,
                left.Boundaries
            );
        }

        public static Parametrisation operator -(
            Parametrisation left, 
            Parametrisation right
            )
        {
            // todo: check boundaries
            return new Parametrisation(
                left.UnboundedU - right.UnboundedU, 
                left.UnboundedV - right.UnboundedV,
                left.Boundaries
            );
        }

        public static Parametrisation operator -(
            Parametrisation left,
            Vector<double> right
            )
        {
            Debug.Assert(right.Count == 2);
            return new Parametrisation(
                left.UnboundedU - right[0],
                left.UnboundedV - right[1],
                left.Boundaries
            );
        }

        public static double DistanceNormMax(
            Parametrisation left,
            Parametrisation right
            )
        {
            // todo: check boundaries
            return Math.Max(
                Math.Abs(left.UnboundedU - right.UnboundedU),
                Math.Abs(left.UnboundedV - right.UnboundedV)
            );
        }
    };

    public interface IParametricSurface
    {
        ParametrisationBoundaries ParametrisationBoundaries { get; }
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