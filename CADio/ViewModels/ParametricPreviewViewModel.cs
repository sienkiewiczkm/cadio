using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using CADio.Helpers.MVVM;
using CADio.Mathematics.Interfaces;
using CADio.Mathematics.Intersections;
using CADio.Mathematics.Trimming;

namespace CADio.ViewModels
{
    public class ParametricPreviewViewModel : INotifyPropertyChanged
    {
        private const int Offset = 8;
        private const int PreviewSize = 512 - 2*Offset;
        private const int SideWidth = Offset * 2 + PreviewSize;

        private ObservableCollection<Parametrisation> _parametricPolygon
            = new ObservableCollection<Parametrisation>();
        private WriteableBitmap _parametricPreviewImage;
        private string _name = "Parametric preview";
        private TrimMode _trimMode;
        private bool _isTrimmingEnabled;
        private RelayCommand _applySurfaceTrimmingCommand;

        public ObservableCollection<Parametrisation> ParametricPolygon
        {
            get { return _parametricPolygon; }
            set { _parametricPolygon = value; OnPropertyChanged(); }
        }

        public PolygonIntersection PolygonIntersection { get; set; }
        public Func<IntersectionParametrisation, Parametrisation> ParamMapping
        {
            get;
            set;
        }

        public ISurfaceTrimmer Trimmer { get; set; }
        public IParametricSurface ObservedSurface { get; set; }

        public WriteableBitmap ParametricPreviewImage
        {
            get { return _parametricPreviewImage; }
            set { _parametricPreviewImage = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }

        public TrimMode TrimMode
        {
            get { return _trimMode; }
            set
            {
                _trimMode = value;
                OnPropertyChanged();

                Trimmer.TrimMode = value;
                RedrawPreview();
                // todo: better handling
            }
        }

        public bool IsTrimmingEnabled => Trimmer != null &&
            ObservedSurface?.Trimmer == Trimmer;

        public bool IsTrimmerAvailable => Trimmer != null;

        public RelayCommand ApplySurfaceTrimmingCommand
        {
            get
            {
                if (_applySurfaceTrimmingCommand == null)
                    _applySurfaceTrimmingCommand = new RelayCommand(
                        ApplySurfaceTrimming,
                        () => !IsTrimmingEnabled
                    );
                return _applySurfaceTrimmingCommand;
            }
        }

        public ParametricPreviewViewModel()
        {
            RedrawPreview();
        }

        public void RedrawPreview()
        {
            var writeableBitmap = BitmapFactory.New(SideWidth, SideWidth);
            using (writeableBitmap.GetBitmapContext())
            {
                writeableBitmap.Clear(Colors.White);
                DrawTrimMap(writeableBitmap);
                DrawGrid(writeableBitmap, PreviewSize, Offset);
                for (var i = 0; i < ParametricPolygon.Count - 1; ++i)
                {
                    DrawLine(
                        writeableBitmap,
                        PreviewSize,
                        Offset,
                        ParametricPolygon[i],
                        ParametricPolygon[i + 1]
                    );
                }
            }

            ParametricPreviewImage = writeableBitmap;
        }

        public void DrawTrimMap(WriteableBitmap bitmap)
        {
            if (Trimmer == null)
                return;

            for (var y = 0; y < PreviewSize; ++y)
            {
                for (var x = 0; x < PreviewSize; ++x)
                {
                    var u = ((double) x)/PreviewSize;
                    var v = ((double) y)/PreviewSize;
                    var color = Trimmer.VerifyParametrisation(u, v)
                        ? Colors.Green
                        : Colors.LightGray;
                    bitmap.SetPixel(Offset + x, Offset + y, color);
                }
            }
        }

        private static void DrawGrid(
            WriteableBitmap writeableBitmap,
            int previewSize,
            int offset)
        {
            const int divisionCells = 10;
            var sideWidth = offset*2 + previewSize;

            for (var i = 0; i <= divisionCells; ++i)
            {
                var additionalOffset = (i*previewSize)/divisionCells;

                writeableBitmap.DrawLine(
                    additionalOffset + offset,
                    offset,
                    additionalOffset + offset,
                    sideWidth - offset,
                    Colors.Gray
                );

                writeableBitmap.DrawLine(
                    offset,
                    additionalOffset + offset,
                    sideWidth - offset,
                    additionalOffset + offset,
                    Colors.Gray
                );
            }
        }

        public static void DrawLine(
            WriteableBitmap writeableBitmap,
            int previewSize,
            int offset,
            Parametrisation begin,
            Parametrisation end
            )
        {
            var x0 = (int)(begin.U*previewSize);
            var y0 = (int)(begin.V*previewSize);
            var x1 = (int)(end.U*previewSize);
            var y1 = (int)(end.V*previewSize);

            var maxDistance = (int) (0.8*previewSize);
            if (Math.Abs(x0 - x1) > maxDistance ||
                Math.Abs(y0 - y1) > maxDistance)
                return;

            writeableBitmap.DrawLine(
                offset + x0,
                offset + y0,
                offset + x1,
                offset + y1,
                Colors.Red
            );
        }

        private void ApplySurfaceTrimming()
        {
            CreateTrimmer();
            ObservedSurface.Trimmer = Trimmer;
            OnPropertyChanged(nameof(IsTrimmerAvailable));
            RedrawPreview();
        }

        private void CreateTrimmer()
        {
            var heavySurfaceTrimmer = new SurfaceTrimmer();
            heavySurfaceTrimmer.BuildTrimmingAreas(
                PolygonIntersection, 
                ParamMapping
            );

            Trimmer = SurfaceTimmerLookup.CreateBasedOn(
                heavySurfaceTrimmer,
                1024,
                1024
            );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(
            [CallerMemberName] string propertyName = null
            )
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName)
            );
        }
    }
}
