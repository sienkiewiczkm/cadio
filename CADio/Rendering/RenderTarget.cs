using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CADio.Geometry;

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

        // currently MUST be black because of 3d and additions
        public static readonly DependencyProperty ClearColorProperty = DependencyProperty.Register(
            "ClearColor", typeof (Color), typeof (RenderTarget), new PropertyMetadata(Colors.Black));

        private WriteableBitmap _leftEyeChannel;
        private WriteableBitmap _rightEyeChannel;
        private WriteableBitmap _outputBitmap;

        public IRenderer Renderer
        {
            get { return (IRenderer)GetValue(RendererProperty); }
            set { SetValue(RendererProperty, value); }
        }

        public bool Enable3D
        {
            get { return (bool)GetValue(Enable3DProperty); }
            set { SetValue(Enable3DProperty, value); }
        }

        public Color ClearColor
        {
            get { return (Color)GetValue(ClearColorProperty); }
            set { SetValue(ClearColorProperty, value); }
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

        public event PropertyChangedEventHandler PropertyChanged;

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

        private RenderedPrimitives GetPixelSpacePrimitives(PerspectiveType perspectiveType)
        {
            if (Renderer == null)
                return new RenderedPrimitives();

            var renderedPrimitives = Renderer.GetRenderedPrimitives(perspectiveType, this);

            var trasnformedPoints = renderedPrimitives.Points
                .Select(point => new Vertex2D(
                    ConvertPointToPixelSpace(point.Position),
                    point.Color
                )).ToList();

            var trasnformedPixels = renderedPrimitives.RawPixels
                .Select(point => new Vertex2D(
                    ConvertPointToPixelSpace(point.Position),
                    point.Color
                )).ToList();

            var transformedLines = renderedPrimitives.Lines
                .Select(line => new Line2D(
                    ConvertPointToPixelSpace(line.From), 
                    ConvertPointToPixelSpace(line.To), 
                    line.Color
                )).ToList();

            return new RenderedPrimitives()
            {
                RawPixels = trasnformedPixels,
                Points = trasnformedPoints,
                Lines = transformedLines,
            };
        }

        public Point ConvertPointToPixelSpace(Point point)
        {
            var x = (+point.X*0.5 + 0.5) * OutputBitmap.PixelWidth;
            var y = (-point.Y*0.5 + 0.5) * OutputBitmap.PixelHeight;
            return new Point(x, y);
        }

        private void RasterizeRenderedPrimitives(WriteableBitmap targetBitmap, RenderedPrimitives pixelSpacePrimitives)
        {
            using (targetBitmap.GetBitmapContext())
            {
                foreach (var line in pixelSpacePrimitives.Lines)
                {
                    targetBitmap.DrawLineAa(
                        (int) line.From.X, (int) line.From.Y,
                        (int) line.To.X, (int) line.To.Y,
                        line.Color);
                }

                foreach (var point in pixelSpacePrimitives.Points)
                {
                    targetBitmap.FillEllipseCentered(
                        (int) point.Position.X, 
                        (int) point.Position.Y, 
                        3, 3, 
                        point.Color
                    );
                }

                foreach (var point in pixelSpacePrimitives.RawPixels)
                {
                    if (point.Position.X < 0 || point.Position.Y < 0 ||
                        (int) point.Position.X >= targetBitmap.PixelWidth ||
                        (int) point.Position.Y >= targetBitmap.PixelHeight)
                    {
                        continue;
                    }

                    targetBitmap.SetPixel(
                        (int)point.Position.X,
                        (int)point.Position.Y,
                        point.Color
                    );
                }
            }
        }

        private void ClearBackground()
        {
            _outputBitmap.Clear(ClearColor);

            if (Enable3D)
            {
                _leftEyeChannel.Clear(ClearColor);
                _rightEyeChannel.Clear(ClearColor);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
