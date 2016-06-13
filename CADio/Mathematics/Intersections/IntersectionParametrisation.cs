using System;
using CADio.Mathematics.Interfaces;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace CADio.Mathematics.Intersections
{
    public struct IntersectionParametrisation
    {
        public Parametrisation First;
        public Parametrisation Second;

        public IntersectionParametrisation(
            Parametrisation first,
            Parametrisation second
            )
        {
            First = first;
            Second = second;
        }

        public IntersectionParametrisation(Vector<double> vector)
        {
            if (vector.Count != 4)
                throw new ArgumentException(
                    "Intersection parametrisation requires exactly 4 parameters"
                    );

            First = new Parametrisation(vector[0], vector[1]);
            Second = new Parametrisation(vector[2], vector[3]);
        }

        public double NormMaximum()
        {
            return Math.Max(
                Math.Abs(First.U),
                Math.Max(
                    Math.Abs(First.V),
                    Math.Max(
                        Math.Abs(Second.U),
                        Math.Abs(Second.V)
                        )
                    )
                );
        }

        public static explicit operator Vector(
            IntersectionParametrisation parametrisation
            )
        {
            return DenseVector.OfArray(
                new []
                {
                    parametrisation.First.U,
                    parametrisation.First.V,
                    parametrisation.Second.U,
                    parametrisation.Second.V
                }
                );
        }

        public static IntersectionParametrisation operator +(
            IntersectionParametrisation left,
            IntersectionParametrisation right
            )
        {
            return new IntersectionParametrisation(
                left.First + right.First,
                left.Second + right.Second
                );
        }

        public static IntersectionParametrisation operator -(
            IntersectionParametrisation left,
            IntersectionParametrisation right
            )
        {
            return new IntersectionParametrisation(
                left.First - right.First,
                left.Second - right.Second
                );
        }
    }
}