using System.ComponentModel;
using System.Runtime.CompilerServices;
using CADio.Geometry.Shapes;
using CADio.SceneManagement;

namespace CADio.ViewModels
{
    internal class SceneTreeViewModel : INotifyPropertyChanged
    {
        private Scene _scene;
        private IShape _selectedShape;
        public event PropertyChangedEventHandler PropertyChanged;

        public Scene Scene
        {
            get { return _scene; }
            set
            {
                _scene = value;
                OnPropertyChanged();
            }
        }

        public IShape SelectedShape
        {
            get { return _selectedShape; }
            set
            {
                _selectedShape = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}