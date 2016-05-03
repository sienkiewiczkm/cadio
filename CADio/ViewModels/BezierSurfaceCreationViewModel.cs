using System.ComponentModel;
using System.Runtime.CompilerServices;
using CADio.Views;

namespace CADio.ViewModels
{
    public class BezierSurfaceCreationViewModel : INotifyPropertyChanged
    {
        private int _segmentsX = 2;
        private int _segmentsY = 2;
        private bool _cylindricalFold;
        private double _radius;
        private double _height;

        public int SegmentsX
        {
            get { return _segmentsX; }
            set { _segmentsX = value; OnPropertyChanged(); }
        }

        public int SegmentsY
        {
            get { return _segmentsY; }
            set { _segmentsY = value; OnPropertyChanged(); }
        }

        public bool CylindricalFold
        {
            get { return _cylindricalFold; }
            set { _cylindricalFold = value; OnPropertyChanged(); }
        }

        public double Radius
        {
            get { return _radius; }
            set { _radius = value; OnPropertyChanged(); }
        }

        public double Height
        {
            get { return _height; }
            set { _height = value; OnPropertyChanged(); }
        }

        public static BezierSurfaceCreationViewModel ShowDialog()
        {
            var viewModel = new BezierSurfaceCreationViewModel();
            var window = new BezierSurfaceC0Creator() {DataContext = viewModel};
            return (window.ShowDialog() ?? false) ? viewModel : null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}