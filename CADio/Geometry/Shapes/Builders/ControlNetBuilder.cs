using System.Collections.Generic;
using System.Windows.Media;
using CADio.SceneManagement.Surfaces;

namespace CADio.Geometry.Shapes.Builders
{
    public class ControlNetBuilder
    {
        private WireframeBuilder _builder;

        public Color NetColor { get; set; }

        public ControlNetBuilder(WireframeBuilder builder)
        {
            _builder = builder;
            NetColor = Colors.LightGreen;
        }

        public void BuildControlNet(
            List<SurfaceControlPoint> controlPoints,
            int controlPointsUAxis,
            bool looped
            )
        {
            var dataRowsCount = controlPoints.Count/controlPointsUAxis;
            var controlPointsVAxis = dataRowsCount + (looped ? 1 : 0);

            for (var x = 0; x < controlPointsUAxis; ++x)
            {
                for (var y = 0; y < controlPointsVAxis; ++y)
                {
                    var ym = y % dataRowsCount;
                    var fromPoint =
                        controlPoints[controlPointsUAxis * ym + x].Position;

                    if (x < controlPointsUAxis - 1)
                    {
                        var toPointIndex = controlPointsUAxis * ym + x + 1;
                        _builder.Connect(fromPoint, NetColor);
                        _builder.Connect(
                            controlPoints[toPointIndex].Position,
                            NetColor
                        );
                        _builder.FinishChain();
                    }

                    if (y < controlPointsVAxis - 1)
                    {
                        var toPointIndex =
                            controlPointsUAxis * ((ym + 1) % dataRowsCount) + x;
                        _builder.Connect(fromPoint, NetColor);
                        _builder.Connect(
                            controlPoints[toPointIndex].Position,
                            NetColor
                        );
                        _builder.FinishChain();
                    }
                }
            }
        }
    }
}