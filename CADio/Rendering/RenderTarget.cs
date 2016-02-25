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
            RasterizeRenderedPrimitives(dc);
        }

        private void RasterizeRenderedPrimitives(DrawingContext dc)
        {
            if (Renderer == null)
                return;

            foreach (var line in Renderer.GetRenderedPrimitives())
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
