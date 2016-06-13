using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using CADio.Mathematics.Interfaces;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace CADio.Mathematics.Intersections
{
    public class DomainIntersectionFinder
    {
        private readonly IParametricSurface[] _surfaces 
            = new IParametricSurface[2];

        public int MaximumNewtonIterations { get; set; } = 300;
        public double MinimumStepLength { get; set; } = 0.0001;
        public double TrackingDistance { get; set; } = 0.05;
        public ISurfaceNearestPointFinder NearestPointFinder { get; set; }

        public PolygonIntersection FindIntersectionPolygon(
            IParametricSurface firstSurface,
            IParametricSurface secondSurface,
            Point3D intersectionNearPoint
            )
        {
            _surfaces[0] = firstSurface;
            _surfaces[1] = secondSurface;

            var firstGuessParametrisation = NearestPointFinder.FindNearest(
                _surfaces[0],
                intersectionNearPoint
            );

            var secondGuessParametrisation = NearestPointFinder.FindNearest(
                _surfaces[1],
                intersectionNearPoint
            );

            var trackingDistanceSetting = TrackingDistance;

            TrackingDistance = -trackingDistanceSetting;
            var polygonPart = GetDirectionalPolygonPart(
                firstGuessParametrisation, 
                secondGuessParametrisation
            );

            if (polygonPart.IsLooped)
                return polygonPart;

            TrackingDistance = trackingDistanceSetting;
            var polygonRemainingPart = GetDirectionalPolygonPart(
                firstGuessParametrisation,
                secondGuessParametrisation
            );

            if (polygonRemainingPart.IsLooped)
                return polygonRemainingPart;

            return PolygonIntersection.JoinFromSamePoint(
                polygonPart,
                polygonRemainingPart
            );
        }

        private PolygonIntersection GetDirectionalPolygonPart(
            Parametrisation firstGuessParametrisation,
            Parametrisation secondGuessParametrisation
            )
        {
            IntersectionParametrisation? previousParametrisation = null;
            var currentIntersectionParametrisation = FindFirstIntersection(
                firstGuessParametrisation,
                secondGuessParametrisation
                );

            var polygon = new PolygonIntersection();

            while (currentIntersectionParametrisation.HasValue &&
                !AreEdgeTrackingBoundaryConditionsFullfilled(
                    previousParametrisation,
                    currentIntersectionParametrisation
                    )
                )
            {
                if (polygon.IsParameterAlreadyFound(
                        currentIntersectionParametrisation.Value
                    ))
                {
                    polygon.IsLooped = true;
                    break;
                }

                polygon.Add(currentIntersectionParametrisation.Value);

                var nextGuess = GetNextInitialGuess(
                    currentIntersectionParametrisation.Value
                );

                if (!nextGuess.HasValue)
                    break;

                previousParametrisation = currentIntersectionParametrisation;
                currentIntersectionParametrisation = FindFirstIntersection(
                    nextGuess.Value.First,
                    nextGuess.Value.Second
                );
            }

            return polygon;
        }

        private bool AreEdgeTrackingBoundaryConditionsFullfilled(
            IntersectionParametrisation? previousParametrisation, 
            IntersectionParametrisation? currentIntersectionParametrisation
            )
        {
            if (!IsValidParametrisation(currentIntersectionParametrisation))
                return true;

            if (!previousParametrisation.HasValue)
                return false;

            if (MinimumStepLength >= (previousParametrisation.Value -
                // ReSharper disable once PossibleInvalidOperationException
                currentIntersectionParametrisation.Value).NormMaximum())
                return true;

            return false;
        }

        private bool IsValidParametrisation(
            IntersectionParametrisation? parametrisation
            )
        {
            return parametrisation.HasValue &&
                IsValidParametrisation(parametrisation.Value.First) &&
                IsValidParametrisation(parametrisation.Value.Second);
        }

        private bool IsValidParametrisation(
            Parametrisation? parametrisation
            )
        {
            return parametrisation.HasValue &&
                parametrisation.Value.U >= 0.0 && 
                parametrisation.Value.U <= 1.0 &&
                parametrisation.Value.V >= 0 &&
                parametrisation.Value.V <= 1.0;
        }            

        private IntersectionParametrisation? FindFirstIntersection(
            Parametrisation startingParametersFirstSurface,
            Parametrisation startingParametersSecondSurface
            )
        {
            var currentParametrisation = new IntersectionParametrisation(
                startingParametersFirstSurface,
                startingParametersSecondSurface
            );

            var iterationNumber = 0;

            do
            {
                var nextParametrisation = EvaluateTwoSurfacesNewtonStep(
                    currentParametrisation
                );

                if (!IsValidParametrisation(nextParametrisation))
                    return null;

                if (MinimumStepLength >= (nextParametrisation - 
                    currentParametrisation).NormMaximum())
                    return nextParametrisation;

                currentParametrisation = nextParametrisation;

            } while (++iterationNumber < MaximumNewtonIterations);

            return null;
        }

        private IntersectionParametrisation? GetNextInitialGuess(
            IntersectionParametrisation previousGuess
            )
        {
            var previousPoint = ((Vector3D)_surfaces[0].Evaluate(
                previousGuess.First
            )).ToMathVector();

            var tangent = EvaluateTangent(previousGuess).ToMathVector();

            var iterationNumber = 0;
            var currentParametrisation = previousGuess;
            do
            {
                var nextParametrisation = EvaluateNextGuessNewtonStep(
                    currentParametrisation,
                    previousPoint,
                    tangent
                );

                if (!IsValidParametrisation(nextParametrisation))
                    return null;

                if (MinimumStepLength >= (nextParametrisation -
                    currentParametrisation).NormMaximum())
                    return nextParametrisation;

                currentParametrisation = nextParametrisation;

            } while (++iterationNumber < MaximumNewtonIterations);

            return null;
        }

        private IntersectionParametrisation EvaluateTwoSurfacesNewtonStep(
            IntersectionParametrisation previousParametrisation
            )
        {
            var evaluatedFunction = 
                EvaluateIntersectionFunction(previousParametrisation)
                .ToMathVector();

            var jacobian = GetJacobian(previousParametrisation);

            return EvaluateNewtonStep(
                previousParametrisation, 
                jacobian, 
                evaluatedFunction
            );
        }

        private IntersectionParametrisation EvaluateNextGuessNewtonStep(
            IntersectionParametrisation previousParametrisation,
            Vector previousPoint,
            Vector tangent
            )
        {
            var evaluated = EvaluateIntersectionWithPlaneFunction(
                previousParametrisation,
                previousPoint,
                tangent
            );

            var jacobian = GetJacobianWithPlane(
                previousParametrisation,
                previousPoint,
                tangent
            );

            return EvaluateNewtonStep(
                previousParametrisation,
                jacobian,
                evaluated
            );
        }

        private Vector3D EvaluateTangent(
            IntersectionParametrisation previousParametrisation
            )
        {
            var normal1 = _surfaces[0].Normal(previousParametrisation.First);
            var normal2 = _surfaces[1].Normal(previousParametrisation.Second);
            var tangent = Vector3D.CrossProduct(normal1, normal2);
            tangent.Normalize();
            return tangent;
        }

        private Matrix GetJacobianWithPlane(
            IntersectionParametrisation previousParametrisation,
            Vector previousPoint,
            Vector tangentVector
            )
        {
            var simpleJacobian = GetJacobian(previousParametrisation);
            var planeJacobian = GetPlaneJacobian(
                previousParametrisation, 
                previousPoint, 
                tangentVector
            );

            return simpleJacobian - planeJacobian;
        }

        private DenseMatrix GetPlaneJacobian(
            IntersectionParametrisation previousParametrisation,
            Vector previousPoint,
            Vector tangentVector
            )
        {
            var planeUDerivative = EvaluateNumericalDerivativeForPlaneEquation(
                _surfaces[0],
                previousParametrisation.First,
                previousPoint,
                tangentVector,
                DerivativeParameter.U
            );

            var planeVDerivative = EvaluateNumericalDerivativeForPlaneEquation(
                _surfaces[0],
                previousParametrisation.First,
                previousPoint,
                tangentVector,
                DerivativeParameter.V
            );

            var planeJacobian = DenseMatrix.OfArray(
                new[,]
                {
                    {planeUDerivative[0], planeVDerivative[0], 0.0, 0.0},
                    {planeUDerivative[1], planeVDerivative[1], 0.0, 0.0},
                    {planeUDerivative[2], planeVDerivative[2], 0.0, 0.0},
                }
            );

            return planeJacobian;
        }

        private static IntersectionParametrisation EvaluateNewtonStep(
            IntersectionParametrisation previousParametrisation,
            Matrix jacobian,
            Vector evaluatedFunction
            )
        {
            var jacobianInv = GetPseudoInverse(jacobian);
            var correction = jacobianInv*evaluatedFunction;
            return previousParametrisation -
                new IntersectionParametrisation(correction);
        }

        private static Matrix<double> GetPseudoInverse(Matrix matrix)
        {
            return matrix
                .Transpose()
                .QR()
                .Solve(DenseMatrix.CreateIdentity(matrix.ColumnCount))
                .Transpose();
        }

        private Vector EvaluateIntersectionWithPlaneFunction(
            IntersectionParametrisation parametrisation,
            Vector previousPoint,
            Vector tangentVector
            )
        {
            var tantentPlaneEquationVector = EvaluatePlaneEquation(
                _surfaces[0],
                parametrisation.First,
                previousPoint, 
                tangentVector
            );

            return EvaluateIntersectionFunction(parametrisation).ToMathVector()
                - tantentPlaneEquationVector;
        }

        private DenseVector EvaluateNumericalDerivativeForPlaneEquation(
            IParametricSurface surface,
            Parametrisation parametrisation,
            Vector previousPoint,
            Vector tangentVector,
            DerivativeParameter parameter
            )
        {
            const double parametrisationStep = 0.1;
            Parametrisation newParametrisation;
            switch (parameter)
            {
                case DerivativeParameter.U:
                    newParametrisation = parametrisation + new Parametrisation(
                        parametrisationStep,
                        0.0
                    );
                    break;
                case DerivativeParameter.V:
                    newParametrisation = parametrisation + new Parametrisation(
                        0.0,
                        parametrisationStep
                    );
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(parameter), 
                        parameter, 
                        null
                    );
            }

            var currentValues = EvaluatePlaneEquation(
                surface,
                parametrisation,
                previousPoint,
                tangentVector
            );

            var shiftedValues = EvaluatePlaneEquation(
                surface,
                newParametrisation,
                previousPoint,
                tangentVector
            );

            return (shiftedValues - currentValues)/parametrisationStep;
        }

        /// <summary>
        /// Calculates F(u,v) = dot(P(u,v) - P, t) - d 
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="parametrisation"></param>
        /// <param name="previousPoint"></param>
        /// <param name="tangentVector"></param>
        /// <returns></returns>
        private DenseVector EvaluatePlaneEquation(
            IParametricSurface surface, 
            Parametrisation parametrisation, 
            Vector previousPoint, 
            Vector tangentVector
            )
        {
            var surfacePosition = 
                ((Vector3D) surface.Evaluate(parametrisation)).ToMathVector();

            var position = surfacePosition - previousPoint;
            var dot = position.DotProduct(tangentVector);
            var tangentZeroEquationValue = dot - TrackingDistance;
            var tantentPlaneEquationVector = DenseVector.OfArray(new[]
            {
                tangentZeroEquationValue,
                tangentZeroEquationValue,
                tangentZeroEquationValue
            });

            return tantentPlaneEquationVector;
        }

        private Vector3D EvaluateIntersectionFunction(
            IntersectionParametrisation parametrisation
            )
        {
            return _surfaces[0].Evaluate(parametrisation.First) - 
                _surfaces[1].Evaluate(parametrisation.Second);
        }

        private DenseMatrix GetJacobian(
            IntersectionParametrisation parametrisation
            )
        {
            var dfU = _surfaces[0].Derivative(
                parametrisation.First, 
                DerivativeParameter.U
            );

            var dfV = _surfaces[0].Derivative(
                parametrisation.First, 
                DerivativeParameter.V
            );

            var dgU = -1.0*_surfaces[1].Derivative(
                parametrisation.Second, 
                DerivativeParameter.U
            );

            var dgV = -1.0*_surfaces[1].Derivative(
                parametrisation.Second, 
                DerivativeParameter.V
            );

            return DenseMatrix.OfArray(new[,]
            {
                {dfU.X, dfV.X, dgU.X, dgV.X}, 
                {dfU.Y, dfV.Y, dgU.Y, dgV.Y}, 
                {dfU.Z, dfV.Z, dgU.Z, dgV.Z},
            });
        }
    }
}