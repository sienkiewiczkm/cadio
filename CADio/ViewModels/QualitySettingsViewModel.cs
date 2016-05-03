using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CADio.ViewModels
{
    public class QualitySettingsViewModel : INotifyPropertyChanged
    {
        private int _surfaceWSubdivisions = 4;
        private int _surfaceHSubdivisions = 4;

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}