using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CADio.ViewModels
{
    public class QualitySettingsViewModel : INotifyPropertyChanged
    {
        private int _surfaceWSubdivisions = 4;
        private int _surfaceHSubdivisions = 4;
        private double _intersectionStep = 0.1;
        private double _equalityEpsilon = 0.01;

        public int SurfaceWSubdivisions
        {
            get { return _surfaceWSubdivisions; }
            set { _surfaceWSubdivisions = value; OnPropertyChanged(); }
        }

        public int SurfaceHSubdivisions
        {
            get { return _surfaceHSubdivisions; }
            set { _surfaceHSubdivisions = value; OnPropertyChanged(); }
        }

        public double IntersectionStep
        {
            get { return _intersectionStep; }
            set { _intersectionStep = value; OnPropertyChanged(); }
        }

        public double EqualityEpsilon
        {
            get { return _equalityEpsilon; }
            set { _equalityEpsilon = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}