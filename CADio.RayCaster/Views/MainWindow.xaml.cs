using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        private int _renderWidth;
        private int _renderHeight;
        private Thread _renderThread;
        private Matrix4X4 _worldTransformation = Matrix4X4.Identity;
        private Point _previousPosition;
        private bool _mouseRotationEnabled;
        private volatile bool _killThread;

        private DisplayMode _displayMode = DisplayMode.ColorWithLighting;
        private double _ellipsoidA = 1;
        private double _ellipsoidB = 1;
        private double _ellipsoidC = 1;
        private int _pixelSizePower = 5;
        private double _exponent = 1;
        private bool _isOnePixelStepEnabled;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public DisplayMode DisplayMode
        {
            get { return _displayMode; }
            set
            {
                _displayMode = value;
                RefreshImage();
                OnPropertyChanged();
            }
        }

        public IEnumerable<DisplayMode> AvailableDisplayModes
            => Enum.GetValues(typeof (DisplayMode)).Cast<DisplayMode>();

        public double EllipsoidA
        {
            get { return _ellipsoidA; }
            set
            {
                _ellipsoidA = value;
                RefreshImage();
                OnPropertyChanged();
            }
        }

        public double EllipsoidB
        {
            get { return _ellipsoidB; }
            set
            {
                _ellipsoidB = value;
                RefreshImage();
                OnPropertyChanged();
            }
        }

        public double EllipsoidC
        {
            get { return _ellipsoidC; }
            set
            {
                _ellipsoidC = value;
                RefreshImage();
                OnPropertyChanged();
            }
        }

        public int PixelSizePower
        {
            get { return _pixelSizePower; }
            set
            {
                _pixelSizePower = value;
                RefreshImage();
                OnPropertyChanged();
                OnPropertyChanged(nameof(PixelSize));
            }
        }

        public int PixelSize => 1 << _pixelSizePower;

        public double Exponent
        {
            get { return _exponent; }
            set
            {
                _exponent = value;
                RefreshImage();
                OnPropertyChanged();
            }
        }

        public bool IsOnePixelStepEnabled
        {
            get { return _isOnePixelStepEnabled; }
            set
            {
                _isOnePixelStepEnabled = value;
                RefreshImage();
                OnPropertyChanged();
            }
        }

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

        private void RenderAdaptively()
        {
            for (var pixelSize = PixelSize; pixelSize > 0; pixelSize = IsOnePixelStepEnabled ? pixelSize - 1 : pixelSize / 2)
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

            var a2 = EllipsoidA * EllipsoidA;
            var b2 = EllipsoidB * EllipsoidB;
            var c2 = EllipsoidC * EllipsoidC;

            var matrix = new Matrix4X4()
            {
                Cells = new double[4, 4]
                {
                    {1/a2, 0, 0, 0},
                    {0, 1/b2, 0, 0},
                    {0, 0, 1/c2, 0},
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
            dot = Math.Pow(dot, Exponent);

            var material = Colors.Yellow;

            //var foundzcolor = (byte) ((foundZ.Value + 1.0)*0.5*255.0);
            //return Color.FromRgb(foundzcolor, foundzcolor, foundzcolor);

            if (DisplayMode == DisplayMode.OnlyNormals)
            {
                var nx = (normal.X + 1.0)*0.5;
                var ny = (normal.Y + 1.0)*0.5;
                var nz = (normal.Z + 1.0)*0.5;
                return Color.FromRgb((byte) (255.0*nx), (byte) (255.0*ny), (byte) (255.0*nz));
            }

            if (DisplayMode == DisplayMode.LightingOnly)
            {
                material = Colors.White;
            }

            return Color.FromRgb((byte)(dot*material.R), (byte)(dot*material.G), (byte)(dot*material.B));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ImageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshImage();
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_mouseRotationEnabled) return;

            var position = e.GetPosition(this);
            var movement = position - _previousPosition;
            _previousPosition = position;

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
