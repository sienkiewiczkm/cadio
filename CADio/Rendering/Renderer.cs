using System;
using System.Collections.Generic;
using System.Linq;
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

        // todo: fixme
        private double _aspectRatio = 1.0;

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

        private bool IsPositionInsideProjectiveCube(Vector4D v)
        {
            for (var i = 0; i < 3; ++i)
                if (-v.W > v[i] || v[i] > v.W) // should be -w <= coord <= w
                    return false;
            return true;
        }

        private bool ClipLineCoordinate(ref Vector4D a, ref Vector4D b, int coordinateIndex, double factor)
        {
            var firstComponent = factor*a[coordinateIndex];
            var firstInside = firstComponent <= a.W;

            var secondComponent = factor*b[coordinateIndex];
            var secondInside = secondComponent <= b.W;

            if (firstInside && secondInside)
                return true;

            if (!firstInside && !secondInside)
                return false;

            var lerpAmt = (a.W - firstComponent)/((a.W - firstComponent) - (b.W - secondComponent));
            var midPoint = a.Lerp(b, lerpAmt);

            if (firstInside)
                b = midPoint;
            if (secondInside)
                a = midPoint;

            return true;
        }

        private bool TransformLineWithClip(Matrix4X4 transformation, ref Vector4D a, ref Vector4D b)
        {
            a = a.Transform(transformation);
            b = b.Transform(transformation);

            for (var i = 0; i < 3; ++i)
            {
                if (!ClipLineCoordinate(ref a, ref b, i, +1.0))
                    return false;
                if (!ClipLineCoordinate(ref a, ref b, i, -1.0))
                    return false;
            }

            a = a.WDivide();
            b = b.WDivide();
            return true;
        }

        public RenderedPrimitives GetRenderedPrimitives(PerspectiveType perspectiveType, RenderTarget renderTarget)
        {
            // todo: fixme
            _aspectRatio = (float) renderTarget.OutputBitmap.PixelWidth/renderTarget.OutputBitmap.PixelHeight;

            var projection = GetProjectionMatrix(perspectiveType);
            var viewProj = projection*Scene.Camera.GetViewMatrix();

            var rasterizedLines = new List<Line2D>();
            var perspectiveColorOverride = GetPerspectiveColorOverride(perspectiveType);

            foreach (var worldObject in Scene.Objects)
                worldObject.PrerenderUpdate();

            if (Scene.GrabbedObjects.Count > 0)
                Scene.Manipulator.Position = Scene.GrabbedObjects.First().Position;

            foreach (var worldObject in Scene.Objects)
            {
                var shape = worldObject.Shape;
                var transformation = viewProj * worldObject.GetWorldMatrix();

                var dynamicShape = shape as IDynamicShape;
                dynamicShape?.UpdateGeometry(
                    /*(p1, p2) => // calculate with clip (only visible length)
                    {
                        var v1 = ((Vector3D) p1).ExtendTo4D();
                        var v2 = ((Vector3D) p2).ExtendTo4D();
                        if (!TransformLineWithClip(transformation, ref v1, ref v2))
                            return 0.0;
                        var pt1 = renderTarget.ConvertPointToPixelSpace((Point) v1);
                        var pt2 = renderTarget.ConvertPointToPixelSpace((Point) v2);
                        return (pt1-pt2).Length;
                    },*/
                    (p1, p2) =>
                    {
                        var v1 = (Point)((Vector3D) p1).ExtendTo4D().Transform(transformation).WDivide();
                        var v2 = (Point)((Vector3D) p2).ExtendTo4D().Transform(transformation).WDivide();
                        return (renderTarget.ConvertPointToPixelSpace(v1)- renderTarget.ConvertPointToPixelSpace(v2)).Length;
                    },
                    p => IsPositionInsideProjectiveCube(((Vector3D)p).ExtendTo4D().Transform(transformation))
                );

                foreach (var segment in shape.Lines)
                {
                    var firstIndex = segment.First;
                    var secondIndex = segment.Second;

                    var v1 = shape.Vertices[firstIndex];
                    var v2 = shape.Vertices[secondIndex];

                    var segmentColor = ColorHelpers.Lerp(v1.Color, v2.Color, 0.5);

                    var firstPos = ((Vector3D) v1.Position).ExtendTo4D();
                    var secondPos = ((Vector3D) v2.Position).ExtendTo4D();
                    if (!TransformLineWithClip(transformation, ref firstPos, ref secondPos))
                        continue;

                    rasterizedLines.Add(new Line2D((Point) firstPos, (Point) secondPos, perspectiveColorOverride ?? segmentColor));
                }
            }

            var rasterizedMarkerPoints = RasterizePoints(viewProj, perspectiveColorOverride, t => t.MarkerPoints);
            var rasterizedRawPixels = RasterizePoints(viewProj, perspectiveColorOverride, t => t.RawPoints);

            return new RenderedPrimitives()
            {
                Points =  rasterizedMarkerPoints,
                Lines = rasterizedLines,
                RawPixels = rasterizedRawPixels,
            };
        }

        private List<Vertex2D> RasterizePoints(Matrix4X4 viewProj, Color? perspectiveColorOverride,
            Func<IShape, IList<Vertex>> vertexExtractorFunc)
        {
            var rasterizedPoints = new List<Vertex2D>();

            foreach (var worldObject in Scene.Objects)
            {
                var shape = worldObject.Shape;
                var transformation = viewProj*worldObject.GetWorldMatrix();

                // todo: renderer probably should not check scene like that
                var colorOverride = worldObject.ColorOverride;
                //if (worldObject == Scene.GrabbedObject)
                //    colorOverride = Colors.Orange;

                var vertexCollection = vertexExtractorFunc(shape);
                foreach (var point in vertexCollection)
                {
                    var pos = ((Vector3D) point.Position).ExtendTo4D().Transform(transformation);
                    // todo: clip
                    if (pos.W < 0) continue;
                    rasterizedPoints.Add(new Vertex2D(
                        (Point) pos.WDivide(),
                        perspectiveColorOverride ?? (colorOverride ?? point.Color)
                    ));
                }
            }

            return rasterizedPoints;
        }

        public Point? GetStandardScreenSpacePosition(Point3D worldPosition)
        {
            var viewProj = GetProjectionMatrix(PerspectiveType.Standard)*Scene.Camera.GetViewMatrix();
            var undivided = ((Vector3D) worldPosition).ExtendTo4D().Transform(viewProj);
            if (undivided.W < 0) return null;
            return (Point)undivided.WDivide();
        }

        private static Color? GetPerspectiveColorOverride(PerspectiveType perspectiveType)
        {
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
            return perspectiveColorOverride;
        }

        private Matrix4X4 GetProjectionMatrix(PerspectiveType perspectiveType)
        {
            var camera = Scene?.Camera;
            if (camera == null)
                return Matrix4X4.Identity;

            var aspectMatrix = Transformations3D.Scaling(new Vector3D(1.0/_aspectRatio, 1.0, 1.0));

            Matrix4X4 projection;
            switch (perspectiveType)
            {
                case PerspectiveType.Standard:
                    projection = aspectMatrix * camera.GetPerspectiveMatrix();
                    break;
                case PerspectiveType.LeftEye:
                    projection = aspectMatrix * camera.GetPerspectiveMatrix(-EyeDistance);
                    break;
                case PerspectiveType.RightEye:
                    projection = aspectMatrix * camera.GetPerspectiveMatrix(+EyeDistance);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(perspectiveType), perspectiveType, null);
            }
            return projection;
        }
    }
}
