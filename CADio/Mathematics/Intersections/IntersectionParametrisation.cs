using System;
using System.Diagnostics;
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

        public bool IsValid()
        {
            return First.IsValid() && Second.IsValid();
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

        public static IntersectionParametrisation operator -(
            IntersectionParametrisation left,
            Vector<double> right
            )
        {
            Debug.Assert(right.Count == 4);
            return new IntersectionParametrisation(
                left.First - DenseVector.OfArray(new[] {right[0], right[1]}),
                left.Second - DenseVector.OfArray(new[] {right[2], right[3]})
            );
        }

        public static double DistanceNormMax(
            IntersectionParametrisation left,
            IntersectionParametrisation right
            )
        {
            return Math.Max(
                Parametrisation.DistanceNormMax(left.First, right.First),
                Parametrisation.DistanceNormMax(left.Second, right.Second)
            );
        }
    }
}