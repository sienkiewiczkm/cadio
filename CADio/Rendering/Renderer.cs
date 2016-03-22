﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.Geometry;
using CADio.Geometry.Shapes;
using CADio.Helpers;
using CADio.Mathematics;
using CADio.SceneManagement;

namespace CADio.Rendering
{
    public class Renderer : IRenderer
    {
        private static Color LeftEyeFilteredColor => Colors.Cyan;
        private static Color RightEyeFilteredColor => Colors.Red;

        private Scene _scene;

        public event EventHandler RenderOutputChanged;

        public double EyeDistance { get; set; } = 0.3;

        public Scene Scene
        {
            get { return _scene; }
            set
            {
                _scene = value;
                ForceRedraw();
            }
        }

        public void ForceRedraw()
        {
            RenderOutputChanged?.Invoke(this, null);
        }

        public RenderedPrimitives GetRenderedPrimitives(PerspectiveType perspectiveType)
        {
            Scene.Camera.ObserverOffset = 2.0;

            Matrix4X4 projection;
            switch (perspectiveType)
            {
                case PerspectiveType.Standard:
                    projection = Transformations3D.SimplePerspective(Scene.Camera.ObserverOffset);
                    break;
                case PerspectiveType.LeftEye:
                    projection = Transformations3D.SimplePerspectiveWithEyeShift(Scene.Camera.ObserverOffset, -EyeDistance);
                    break;
                case PerspectiveType.RightEye:
                    projection = Transformations3D.SimplePerspectiveWithEyeShift(Scene.Camera.ObserverOffset, +EyeDistance);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(perspectiveType), perspectiveType, null);
            }

            var viewProj = projection*Scene.Camera.GetViewMatrix();

            var rasterizedLines = new List<Line2D>();

            Color? perspectiveColorOverride = null;
            switch (perspectiveType)
            {
                case PerspectiveType.LeftEye:
                    perspectiveColorOverride = RightEyeFilteredColor;
                    break;
                case PerspectiveType.RightEye:
                    perspectiveColorOverride = LeftEyeFilteredColor;
                    break;
            }

            foreach (var worldObject in Scene.Objects)
            {
                var shape = worldObject.Shape;
                var transformation = viewProj * worldObject.GetWorldMatrix();
                foreach (var segment in shape.Segments)
                {
                    var firstIndex = segment.First;
                    var secondIndex = segment.Second;

                    var v1 = shape.Vertices[firstIndex];
                    var v2 = shape.Vertices[secondIndex];

                    var firstPos = ((Vector3D) v1.Position).ExtendTo4D().Transform(transformation);
                    var secondPos = ((Vector3D) v2.Position).ExtendTo4D().Transform(transformation);

                    if (firstPos.W < 0 && secondPos.W < 0)
                        continue;

                    if (firstPos.W < 0 || secondPos.W < 0)
                    {
                        continue;
                    }

                    var segmentColor = ColorHelpers.Lerp(v1.Color, v2.Color, 0.5);

                    firstPos = firstPos.WDivide();
                    secondPos = secondPos.WDivide();

                    rasterizedLines.Add(new Line2D((Point) firstPos, (Point) secondPos, perspectiveColorOverride ?? segmentColor));
                }
            }

            var rasterizedPoints = new List<Vertex2D>();
            foreach (var worldObject in Scene.Objects)
            {
                var shape = worldObject.Shape;
                var transformation = viewProj * worldObject.GetWorldMatrix();

                foreach (var markerPoint in shape.MarkerPoints)
                {
                    var pos = ((Vector3D) markerPoint.Position).ExtendTo4D().Transform(transformation);
                    // todo:clip
                    if (pos.W < 0) continue;
                    rasterizedPoints.Add(new Vertex2D(
                        (Point) pos.WDivide(),
                        markerPoint.Color
                    ));
                }
            }

            return new RenderedPrimitives()
            {
                Points =  rasterizedPoints,
                Lines = rasterizedLines,
            };
        }
    }
}
