using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CADio.Rendering
{
    public class RenderTarget : Control
    {
        public static readonly DependencyProperty RendererProperty = DependencyProperty.Register(
            "Renderer", typeof(IRenderer), typeof(RenderTarget), 
            new FrameworkPropertyMetadata(default(IRenderer), FrameworkPropertyMetadataOptions.AffectsRender));

        public IRenderer Renderer
        {
            get { return (IRenderer) GetValue(RendererProperty); }
            set { SetValue(RendererProperty, value); }
        }

        public void RequestRedraw()
        {
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            ClearBackground(dc);
            var pixelSpacePrimitives = GetPixelSpacePrimitives();
            RasterizeRenderedPrimitives(dc, pixelSpacePrimitives);
        }

        private IEnumerable<Line2D> GetPixelSpacePrimitives()
        {
            if (Renderer == null)
                return new List<Line2D>();

            return Renderer.GetRenderedPrimitives()
                .Select(line => new Line2D(
                    ConvertPointToPixelSpace(line.From), ConvertPointToPixelSpace(line.To), line.Color
                )).ToList();
        }

        private Point ConvertPointToPixelSpace(Point point)
        {
            var x = (point.X*0.5 + 0.5) * ActualWidth;
            var y = (point.Y*0.5 + 0.5) * ActualHeight;
            return new Point(x, y);
        }

        private void RasterizeRenderedPrimitives(DrawingContext dc, IEnumerable<Line2D> pixelSpacePrimitives)
        {
            foreach (var line in pixelSpacePrimitives)
            {
                dc.DrawLine(new Pen() {Brush = new SolidColorBrush(line.Color)}, line.From, line.To);
            }
        }

        private void ClearBackground(DrawingContext dc)
        {
            var wholeAreaRect = new Rect(new Point(0, 0), new Size(ActualWidth, ActualHeight));
            dc.PushClip(new RectangleGeometry(wholeAreaRect));
            dc.DrawRectangle(Background, null, wholeAreaRect);
        }
    }
}
