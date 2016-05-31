using System;
using CADio.Configuration;
using CADio.Mathematics.Interfaces;

namespace CADio.Geometry.Shapes.Builders
{
    public class SurfaceConstantParameterLinesBuilder
    {
        private WireframeBuilder _builder;
        private IParametricSurface _sampledSurface;

        public SurfaceConstantParameterLinesBuilder(WireframeBuilder builder)
        {
            _builder = builder;
        }

        public void Build(IParametricSurface surface)
        {
            _sampledSurface = surface;

            var samplesU =
                GlobalSettings.QualitySettingsViewModel.SurfaceHSubdivisions;
            var samplesV =
                GlobalSettings.QualitySettingsViewModel.SurfaceWSubdivisions;
            var lineSamples = 32;

            Func<int, double> uCoordGenerator = (i) => i/(samplesU - 1.0);
            Func<int, double> vCoordGenerator = (i) => i/(samplesV - 1.0);
            Func<int, double> lineGenerator = (i) => i/(lineSamples - 1.0);

            for (var i = 0; i < samplesU; ++i)
            {
                var uCoord = uCoordGenerator(i);
                ScanConstantParameter(
                    (unused) => uCoord,
                    lineGenerator,
                    lineSamples
                );
            }

            for (var i = 0; i < samplesV; ++i)
            {
                var vCoord = vCoordGenerator(i);
                ScanConstantParameter(
                    lineGenerator,
                    (unused) => vCoord,
                    lineSamples
                );
            }

        }

        protected void ScanConstantParameter(
            Func<int, double> uCoordGenerator,
            Func<int, double> vCoordGenerator,
            int samples)
        {
            for (var i = 0; i < samples; ++i)
            {
                _builder.Connect(
                    _sampledSurface.Evaluate(
                        uCoordGenerator(i),
                        vCoordGenerator(i)
                    )
                );
            }

            _builder.FinishChain();
        }
    }
}