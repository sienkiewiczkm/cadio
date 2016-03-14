using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CADio.Rendering
{
    public class RenderTarget : Image, INotifyPropertyChanged
    {
        public static readonly DependencyProperty RendererProperty = DependencyProperty.Register(
            "Renderer", typeof(IRenderer), typeof(RenderTarget), 
            new FrameworkPropertyMetadata(default(IRenderer), 
                FrameworkPropertyMetadataOptions.AffectsRender,
                SetupRendererEventSubscription));

        public static readonly DependencyProperty Enable3DProperty = DependencyProperty.Register(
            "Enable3D", typeof (bool), typeof (RenderTarget), new FrameworkPropertyMetadata(default(bool), 
                FrameworkPropertyMetadataOptions.AffectsRender,
                RequestRedrawOnDependencyChange));

        private WriteableBitmap _leftEyeChannel;
        private WriteableBitmap _rightEyeChannel;
        private WriteableBitmap _outputBitmap;

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var width = (int)arrangeSize.Width;
            var height = (int)arrangeSize.Height;
            if (_outputBitmap == null || _outputBitmap.PixelWidth != width || _outputBitmap.PixelHeight != height)
            {
                CreateOutputBitmaps(width, height);
            }

            return base.ArrangeOverride(arrangeSize);
        }

        private void CreateOutputBitmaps(int width, int height)
        {
            _outputBitmap = BitmapFactory.New(width, height);
            _leftEyeChannel = BitmapFactory.New(width, height);
            _rightEyeChannel = BitmapFactory.New(width, height);

            Source = _outputBitmap;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IRenderer Renderer
        {
            get { return (IRenderer) GetValue(RendererProperty); }
            set { SetValue(RendererProperty, value); }
        }

        public bool Enable3D
        {
            get { return (bool)GetValue(Enable3DProperty); }
            set { SetValue(Enable3DProperty, value); }
        }

        public WriteableBitmap OutputBitmap
        {
            get { return _outputBitmap; }
            set
            {
                _outputBitmap = value;
                OnPropertyChanged();
            }
        }

        private static void SetupRendererEventSubscription(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var renderTarget = (RenderTarget) obj;
            var oldValue = (IRenderer) e.OldValue;
            var newValue = (IRenderer) e.NewValue;

            if (oldValue != null)
            {
                oldValue.RenderOutputChanged -= renderTarget.OnRenderOutputChanged;
            }

            if (newValue != null)
            {
                newValue.RenderOutputChanged += renderTarget.OnRenderOutputChanged;
            }
        }

        private static void RequestRedrawOnDependencyChange(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (obj as RenderTarget)?.RequestRedraw();
        }

        public void RequestRedraw()
        {
            ClearBackground();

            if (!Enable3D)
            {
                RasterizeRenderedPrimitives(_outputBitmap, GetPixelSpacePrimitives(PerspectiveType.Standard));
                return;
            }

            RasterizeRenderedPrimitives(_leftEyeChannel, GetPixelSpacePrimitives(PerspectiveType.LeftEye));
            RasterizeRenderedPrimitives(_rightEyeChannel, GetPixelSpacePrimitives(PerspectiveType.RightEye));

            using (_outputBitmap.GetBitmapContext())
            {
                _outputBitmap.Blit(new Point(0, 0), _leftEyeChannel,
                    new Rect(new Point(0, 0), new Size(_leftEyeChannel.Width, _leftEyeChannel.Height)),
                    Colors.White, WriteableBitmapExtensions.BlendMode.Additive);

                _outputBitmap.Blit(new Point(0, 0), _rightEyeChannel,
                    new Rect(new Point(0, 0), new Size(_rightEyeChannel.Width, _rightEyeChannel.Height)),
                    Colors.White, WriteableBitmapExtensions.BlendMode.Additive);
            }
        }

        private void OnRenderOutputChanged(object sender, EventArgs e)
        {
            RequestRedraw();
        }

        private IEnumerable<Line2D> GetPixelSpacePrimitives(PerspectiveType perspectiveType)
        {
            if (Renderer == null)
                return new List<Line2D>();

            return Renderer.GetRenderedPrimitives(perspectiveType)
                .Select(line => new Line2D(
                    ConvertPointToPixelSpace(line.From), 
                    ConvertPointToPixelSpace(line.To), 
                    line.Color
                )).ToList();
        }

        private Point ConvertPointToPixelSpace(Point point)
        {
            var aspect = ((float)OutputBitmap.PixelWidth)/OutputBitmap.PixelHeight;
            var x = ((point.X/aspect)*0.5 + 0.5) * OutputBitmap.PixelWidth;
            var y = (-point.Y*0.5 + 0.5) * OutputBitmap.PixelHeight;
            return new Point(x, y);
        }

        private void RasterizeRenderedPrimitives(WriteableBitmap targetBitmap, IEnumerable<Line2D> pixelSpacePrimitives)
        {
            using (targetBitmap.GetBitmapContext())
            {
                foreach (var line in pixelSpacePrimitives)
                {
                    targetBitmap.DrawLineAa(
                        (int)line.From.X, (int)line.From.Y, 
                        (int)line.To.X,   (int)line.To.Y, 
                        line.Color);
                }
            }
        }

        private void ClearBackground()
        {
            _outputBitmap.Clear(Colors.Black);

            if (Enable3D)
            {
                _leftEyeChannel.Clear(Colors.Black);
                _rightEyeChannel.Clear(Colors.Black);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
