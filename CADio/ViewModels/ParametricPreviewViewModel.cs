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
using CADio.Mathematics.Interfaces;

namespace CADio.ViewModels
{
    public class ParametricPreviewViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Parametrisation> _parametricPolygon
            = new ObservableCollection<Parametrisation>();
        private WriteableBitmap _parametricPreviewImage;
        private string _name = "Parametric preview";

        public ObservableCollection<Parametrisation> ParametricPolygon
        {
            get { return _parametricPolygon; }
            set { _parametricPolygon = value; OnPropertyChanged(); }
        }

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

        public ParametricPreviewViewModel()
        {
            RedrawPreview();
        }

        public void RedrawPreview()
        {
            const int offset = 8;
            const int previewSize = 500;
            const int sideWidth = offset*2 + previewSize;

            var writeableBitmap = BitmapFactory.New(sideWidth, sideWidth);
            using (writeableBitmap.GetBitmapContext())
            {
                writeableBitmap.Clear(Colors.White);
                DrawGrid(writeableBitmap, previewSize, offset);
                for (var i = 0; i < ParametricPolygon.Count - 1; ++i)
                {
                    DrawLine(
                        writeableBitmap,
                        previewSize,
                        offset,
                        ParametricPolygon[i],
                        ParametricPolygon[i + 1]
                    );
                }
            }

            ParametricPreviewImage = writeableBitmap;
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

            writeableBitmap.DrawLineAa(
                offset + x0,
                offset + y0,
                offset + x1,
                offset + y1,
                Colors.Red,
                3
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
