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
        private Matrix4X4 _worldTransformation = Matrix4X4.Identity;

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
        private bool _mouseRotationEnabled;

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

            var a = 1.0;
            var b = 0.8;
            var c = 1.0;

            var matrix = new Matrix4X4()
            {
                Cells = new double[4, 4]
                {
                    {1/(a*a), 0, 0, 0},
                    {0, 1/(b*b), 0, 0},
                    {0, 0, 1/(c*c), 0},
                    {0, 0, 0, -1}
                }
            };

            var inversedWorld = _worldTransformation.GaussianInverse();
            var dm = inversedWorld.Transpose() * matrix * inversedWorld;

            var vec = new Vector4D(rayx, rayy, 0.0, 1.0);

            var foundZ = ImplicitEllipsoidEquationSolver.SolveZ(vec, dm);
            if (!foundZ.HasValue) return Colors.Black;

            if (x == maxX/2 && y == maxY/2)
            {
                Console.WriteLine("debugpoint");
            }

            var crossPoint = new Vector4D(rayx, rayy, foundZ.Value, 1.0);
            var normal = ImplicitEllipsoidEquationSolver.CalculateNormal(crossPoint, dm);
            var lightVec = new Vector4D(0.0, 0.0, -1.0, 0.0);
            var dot = Math.Max(0, normal.DotProduct(lightVec));
            var material = Colors.Yellow;

            //var foundzcolor = (byte) ((foundZ.Value + 1.0)*0.5*255.0);
            //return Color.FromRgb(foundzcolor, foundzcolor, foundzcolor);

            bool showNormals = false;
            if (showNormals)
            {
                return Color.FromRgb((byte) (255.0*normal.X), (byte) (255.0*normal.Y), (byte) (255.0*normal.Z));
            }

            return Color.FromRgb((byte)(dot*material.R), (byte)(dot*material.G), (byte)(dot*material.B));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Point _previousPosition;
        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_mouseRotationEnabled) return;

            var position = e.GetPosition(this);
            var movement = position - _previousPosition;

            var rotationMatrix = Transformations3D.RotationY(0.005*movement.X)
                                 *Transformations3D.RotationX(0.005*movement.Y);

            _worldTransformation = rotationMatrix*_worldTransformation;

            RefreshImage();
        }

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _mouseRotationEnabled = true;
            _previousPosition = e.GetPosition(this);
        }

        private void OnMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _mouseRotationEnabled = false;
        }

        private void OnMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var scalingFactor = Math.Max(0, 1.0 + e.Delta*0.002);
            var scalingMatrix = Transformations3D.Scaling(scalingFactor);
            _worldTransformation = scalingMatrix*_worldTransformation;

            RefreshImage();
        }
    }
}
