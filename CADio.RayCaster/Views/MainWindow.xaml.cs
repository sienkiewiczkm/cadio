using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CADio.Mathematics;
using CADio.RayCaster.Utils;

namespace CADio.RayCaster.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private WriteableBitmap _writeableBitmap;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void ImageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshImage();
        }

        private int _renderWidth;
        private int _renderHeight;
        private Thread _renderThread;

        private void RefreshImage()
        {
            _renderWidth = (int) OutputCanvas.ActualWidth;
            _renderHeight = (int) OutputCanvas.ActualHeight;

            if (_renderThread != null)
            {
                _killThread = true;
                _renderThread.Join();
                _killThread = false;
            }

            _renderThread = new Thread(RenderAdaptively);
            _renderThread.Start();
        }

        private volatile bool _killThread;
        private void RenderAdaptively()
        {
            const int pixelStartingSizePow2 = 32;
            for (var pixelSize = pixelStartingSizePow2; pixelSize > 0; --pixelSize)
            {
                var output = BitmapFactory.New(_renderWidth, _renderHeight);

                using (output.GetBitmapContext())
                {
                    output.Clear(Colors.Blue);

                    var col = (Color.FromRgb(((byte) ((pixelSize - 1)*8)), 0, 0));

                    for (var yblock = 0; yblock < (output.PixelHeight + pixelSize - 1)/pixelSize; ++yblock)
                    {
                        for (var xblock = 0; xblock < (output.PixelWidth + pixelSize - 1)/pixelSize; ++xblock)
                        {
                            if (_killThread) return;

                            var xc = Math.Min(_renderWidth - 1, xblock*pixelSize + (pixelSize/2));
                            var yc = Math.Min(_renderHeight - 1, yblock*pixelSize + (pixelSize/2));
                            var blockColor = ShootRay(xc, yc, _renderWidth - 1, _renderHeight - 1);

                            output.FillRectangle(xblock*pixelSize, yblock*pixelSize,
                                Math.Min(output.PixelWidth - 1, (xblock + 1)*pixelSize - 1) + 1,
                                Math.Min(output.PixelHeight - 1, (yblock + 1)*pixelSize - 1) + 1,
                                blockColor
                            );
                        }
                    }
                }

                output.Freeze();
                Dispatcher.BeginInvoke(new Action(() => ImageSource.Source = output));
            }
        }

        private Color ShootRay(int x, int y, int maxX, int maxY)
        {
            var aspect = (double) maxX/maxY;

            var rayx = aspect*(2.0*x/maxX - 1.0);
            var rayy = (double) y/maxY*2.0 - 1.0;

            var matrix = new Matrix4x4()
            {
                Cells = new double[4, 4]
                {
                    {1, 0, 0, 0},
                    {0, 2, 0, 0},
                    {0, 0, 1, 0},
                    {0, 0, 0, -1}
                }
            };

            var vec = new Vector4D(rayx, rayy, 0.0, 1.0);

            var solution = ImplicitEllipsoidEquationSolver.SolveZ(vec, matrix);
            if (solution.HasValue)
                return Colors.Yellow;

            return Colors.Black;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
